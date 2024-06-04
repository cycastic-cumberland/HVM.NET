#!/bin/sh
cargo build --release
strip `dirname "$0"`/target/x86_64-unknown-linux-gnu/release/libhvm_dotnet.so