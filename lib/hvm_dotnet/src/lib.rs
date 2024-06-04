use std::ffi::{CStr, CString};
use std::os::raw::c_char;
use crate::ast::Book;

mod hvm;
mod ast;

#[cfg(feature = "c")]
extern "C" {
    fn hvm_c(book_buffer: *const u32, enable_mem_dump: u32) -> *mut EvaluationResultRaw;
}

#[repr(u32)]
pub enum RuntimeTypes {
    RUST = 0,
    C = 1
}

#[repr(C)]
pub struct EvaluationResultRaw {
    iterations: u64,
    time: f64,
    result: *mut c_char,
    mem_dump: *mut c_char,
    deallocator: extern "C" fn(*mut EvaluationResultRaw) -> ()
}

struct EvaluationResult {
    iterations: u64,
    time: f64,
    result: String,
    mem_dump: String,
}

fn rust_evaluate(book: &hvm::Book, enable_mem_dump: u32) -> Result<EvaluationResult, CString> {
    // Initializes the global net
    let net = hvm::GNet::new(1 << 29, 1 << 29);

    // Initializes threads
    let mut tm = hvm::TMem::new(0, 1);

    // Creates an initial redex that calls main
    let main_id = match book.defs.iter().position(|def| def.name == "main"){
        Some(v) => v,
        None => { return Err(CString::new("No main function found").unwrap()); }
    };
    tm.rbag.push_redex(hvm::Pair::new(hvm::Port::new(hvm::REF, main_id as u32), hvm::ROOT));
    net.vars_create(hvm::ROOT.get_val() as usize, hvm::NONE);

    // Starts the timerCUDA compiler not foundCUDA compiler not found
    let start = std::time::Instant::now();

    // Evaluates
    tm.evaluator(&net, &book);

    // Stops the timer
    let duration = start.elapsed();

    let result: String;
    if let Some(tree) = ast::Net::readback(&net, book) {
        result = tree.show();
    } else {
        result = String::default();
    }

    // Prints interactions and time
    let iterations = net.itrs.load(std::sync::atomic::Ordering::Relaxed);
    let duration_secs = duration.as_secs_f64();
    // let mips = iterations as f64 / duration.as_secs_f64() / 1_000_000.0;
    let mem_dump = if enable_mem_dump > 0 { net.show() } else { String::default() };
    return Ok(EvaluationResult {
        iterations,
        time: duration_secs,
        result,
        mem_dump,
    });
}

unsafe fn c_evaluate(book_ptr: *const hvm::Book, enable_mem_dump: u32, err_out: *mut *mut c_char) -> *mut EvaluationResultRaw {
    #[cfg(feature = "c")]{
        let mut data : Vec<u8> = Vec::new();
        let book = &*book_ptr;
        book.to_buffer(&mut data);
        return hvm_c(data.as_mut_ptr() as *const u32, enable_mem_dump);
    }
    *err_out = CString::new("C runtime not supported").unwrap().into_raw();
    return 0usize as *mut EvaluationResultRaw;
}

extern "C" fn drop_evaluation_result_raw(this_ptr: *mut EvaluationResultRaw){
    unsafe {
        let this = Box::from_raw(this_ptr);
        free_cstring(this.result);
        free_cstring(this.mem_dump);
    }
}

#[no_mangle]
pub unsafe extern "C" fn free_cstring(string_ptr: *mut c_char) {
    _ = CString::from_raw(string_ptr);
}

#[no_mangle]
pub unsafe extern "C" fn free_book(book_ptr: *mut hvm::Book){
    _ = Box::from_raw(book_ptr);
}

#[no_mangle]
pub unsafe extern "C" fn book_parse(code: *const c_char, err_out: *mut *mut c_char) -> *mut hvm::Book {
    *err_out = 0usize as *mut c_char;
    let c_str = CStr::from_ptr(code);
    let converted_str = match c_str.to_str(){
        Ok(value) => value,
        Err(err) => {
            *err_out = CString::new(err.to_string()).unwrap().into_raw();
            return 0usize as *mut hvm::Book;
        }
    };
    return match Book::parse(converted_str){
        Ok(value) => Box::into_raw(Box::new(match value.build() {
            Ok(v) => v,
            Err(err) => {
                *err_out = CString::new(err.to_string()).unwrap().into_raw();
                return 0usize as *mut hvm::Book;
            }
        })),
        Err(err) => {
            *err_out = CString::new(err.to_string()).unwrap().into_raw();
            0usize as *mut hvm::Book
        }
    }
}

#[no_mangle]
pub unsafe extern "C" fn book_evaluate(book_ptr: *const hvm::Book, runtime_type: RuntimeTypes, enable_mem_dump: u32, err_out: *mut *mut c_char) -> *mut EvaluationResultRaw {
    let book: &hvm::Book = &*book_ptr;
    *err_out = 0usize as *mut c_char;
    match runtime_type {
        RuntimeTypes::RUST => {
            match rust_evaluate(&book, enable_mem_dump) {
                Ok(EvaluationResult{
                       iterations, time, result, mem_dump
                   }) => Box::into_raw(Box::from(EvaluationResultRaw{
                    iterations,
                    time,
                    result: CString::new(result).unwrap().into_raw(),
                    mem_dump: CString::new(mem_dump).unwrap().into_raw(),
                    deallocator: drop_evaluation_result_raw
                })),
                Err(e) => {
                    *err_out = e.into_raw();
                    0usize as *mut EvaluationResultRaw
                }
            }
        }
        RuntimeTypes::C => c_evaluate(book_ptr, enable_mem_dump, err_out),
        _ => {
            let e_str = format!("Invalid runtime type: {}", runtime_type as u32);
            *err_out = CString::new(e_str).unwrap().into_raw();
            0usize as *mut EvaluationResultRaw
        }
    }
}

#[no_mangle]
pub unsafe extern "C" fn free_evaluation_result(result_ptr: *mut EvaluationResultRaw){
    let this = &*result_ptr;
    let deallocator = this.deallocator;
    deallocator(result_ptr);
}

#[no_mangle]
pub unsafe extern "C" fn book_serialize(book_ptr: *const hvm::Book, err_out: *mut *mut c_char) -> *mut Vec<u8>{
    let mut data : Vec<u8> = Vec::new();
    let book = &*book_ptr;
    match book.to_buffer_safe(&mut data){
        Ok(_) => {}
        Err(e) => {
            *err_out = CString::new(e).unwrap().into_raw();
            return 0usize as *mut Vec<u8>;
        }
    }
    return Box::into_raw(Box::new(data));
}

#[no_mangle]
pub unsafe extern "C" fn vec_get_length(vec_ptr: *mut Vec<u8>) -> u64 {
    let vec = Box::from_raw(vec_ptr);
    let length = vec.len() as u64;
    _ = Box::into_raw(vec);
    length
}

#[no_mangle]
pub unsafe extern "C" fn vec_copy(vec_ptr: *mut Vec<u8>, loc: *mut u8, size: u64) {
    let vec = Box::from_raw(vec_ptr);
    let slice = std::slice::from_raw_parts_mut(loc, size as usize);
    slice[..vec.len()].copy_from_slice(vec.as_slice());
    _ = Box::into_raw(vec);
}

#[no_mangle]
pub unsafe extern "C" fn free_vec(vec_ptr: *mut Vec<u8>){
    _ = Box::from_raw(vec_ptr);
}
