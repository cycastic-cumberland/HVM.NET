#ifndef HVM_H
#define HVM_H

#include <inttypes.h>
#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

// Integers
// --------

typedef uint8_t bool;

typedef uint8_t u8;
typedef uint16_t u16;
typedef int32_t i32;
typedef uint32_t u32;
typedef uint64_t u64;
typedef float f32;
typedef double f64;

typedef _Atomic (u8) a8;
typedef _Atomic (u16) a16;
typedef _Atomic (u32) a32;
typedef _Atomic (u64) a64;

typedef struct {
    u64 iterations;
    f64 time;
    char *result;
    char *mem_dump;

    void (*deallocator)(void *);
} EvaluationResultRaw;

void c_free_evaluation_result(void *result_ptr) {
    EvaluationResultRaw *casted = (EvaluationResultRaw *) result_ptr;
    if (casted->result)
        free(casted->result);
    if (casted->mem_dump)
        free(casted->mem_dump);
    free(result_ptr);
}

typedef struct {
    char *buffer;
    u32 count;
    u32 capacity;
} StringBuffer;

void string_buffer_init(StringBuffer *buffer, u32 reserved) {
    if (!reserved) {
        buffer->buffer = (char *) NULL;
    } else {
        buffer->buffer = malloc(sizeof(char) * reserved);
    }
    buffer->count = 0;
    buffer->capacity = reserved;
}

void string_buffer_free(StringBuffer *buffer) {
    free(buffer->buffer);
    buffer->buffer = (char *) NULL;
    buffer->count = 0;
    buffer->capacity = 0;
}

void string_buffer_ensure_amount(StringBuffer *buffer, u32 amount) {
    u32 target_cap = buffer->count + amount;
    if (target_cap <= buffer->capacity) return;
    u32 new_cap = buffer->capacity * 2;
    char *new_buffer = (char *) malloc(sizeof(char) * new_cap);
    memcpy(new_buffer, buffer->buffer, sizeof(char) * buffer->count);
    free(buffer->buffer);
    buffer->buffer = new_buffer;
    buffer->capacity = new_cap;
}

char *string_buffer_consume(StringBuffer *buffer) {
    u32 length = buffer->count;
    char *c_str = (char *) malloc(sizeof(char) * (length + 1));
    memcpy(c_str, buffer->buffer, sizeof(char) * length);
    c_str[length] = '\0';
    string_buffer_free(buffer);
    return c_str;
}

void string_buffer_write_u32_custom(StringBuffer *buffer, const char* format, u32 value) {
    // 4294967295.length() == 10
    string_buffer_ensure_amount(buffer, 10);
    char* loc = &buffer->buffer[buffer->count];
    i32 written = sprintf(loc, format, value);
    buffer->count += written;
}

void string_buffer_write_u32(StringBuffer *buffer, u32 value) {
    string_buffer_write_u32_custom(buffer, "%u", value);
}

void string_buffer_write_u32_hex(StringBuffer *buffer, u32 value) {
    string_buffer_write_u32_custom(buffer, "x%x", value);
}

void string_buffer_write_u64(StringBuffer *buffer, u64 value) {
    // 18446744073709551615.length() == 20
    string_buffer_ensure_amount(buffer, 20);
    char *loc = &buffer->buffer[buffer->count];
    i32 written = sprintf(loc, "%lu", value);
    buffer->count += written;
}

void string_buffer_write_f32_custom(StringBuffer *buffer, const char* format, f32 value) {
    // (-1.7976931348623157e+308).length() == 24
    string_buffer_ensure_amount(buffer, 24);
    char *loc = &buffer->buffer[buffer->count];
    i32 written = sprintf(loc, format, value);
    buffer->count += written;
}

void string_buffer_write_f64(StringBuffer *buffer, f64 value) {
    // (-1.7976931348623157e+308).length() == 24
    string_buffer_ensure_amount(buffer, 24);
    char *loc = &buffer->buffer[buffer->count];
    i32 written = sprintf(loc, "%f", value);
    buffer->count += written;
}

void string_buffer_write_char(StringBuffer *buffer, char value) {
    string_buffer_ensure_amount(buffer, 1);
    char *loc = &buffer->buffer[buffer->count];
    i32 written = sprintf(loc, "%c", value);
    buffer->count += written;
}

void string_buffer_write_string(StringBuffer *buffer, char *str) {
    u32 count = 0;
    while (str[count++] != '\0') {}
    count--;
    string_buffer_ensure_amount(buffer, count);
    memcpy(&buffer->buffer[count], str, sizeof(char) * count);
    buffer->count += count;
}

#endif