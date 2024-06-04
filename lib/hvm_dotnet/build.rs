//   Copyright 2023-2024 Higher Order Company
//   Copyright 2024 Nguyễn Khánh Nam
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//
//   Copied and adapted from Higher-order Virtual Machine 2: 
//      https://github.com/HigherOrderCO/HVM.git

fn main() {
    let cores = num_cpus::get();
    let tpcl2 = (cores as f64).log2().floor() as u32;

    println!("cargo:rerun-if-changed=src/hvm.c");
    println!("cargo:rerun-if-changed=src/hvm.cu");

    match cc::Build::new()
        .file("src/hvm.c")
        .opt_level(3)
        .warnings(false)
        .define("TPC_L2", &*tpcl2.to_string())
        .try_compile("hvm-c") {
        Ok(_) => println!("cargo:rustc-cfg=feature=\"c\""),
        Err(e) => {
            println!("cargo:warning=\x1b[1m\x1b[31mWARNING: Failed to compile hvm.c:\x1b[0m {}", e);
            println!("cargo:warning=Ignoring hvm.c and proceeding with build. \x1b[1mThe C runtime will not be available.\x1b[0m");
        }
    }
}
