warning: unused variable: `rust_encodings`
  --> src/encoding/methods.rs:26:9
   |
26 |     let rust_encodings: Vec<tokenizers::Encoding> = Vec::with_capacity(count);
   |         ^^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_rust_encodings`
   |
   = note: `#[warn(unused_variables)]` on by default
warning: unused variable: `c_encoding`
  --> src/encoding/methods.rs:34:13
   |
34 |         let c_encoding = unsafe { &**encoding_ptr };
   |             ^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_c_encoding`
warning: unused variable: `growing_offsets`
  --> src/encoding/methods.rs:13:5
   |
13 |     growing_offsets: bool,
   |     ^^^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_growing_offsets`
warning: unused variable: `len_ptr`
  --> src/encoding/methods.rs:14:5
   |
14 |     len_ptr: *mut usize,
   |     ^^^^^^^ help: if this is intentional, prefix it with an underscore: `_len_ptr`
warning: unused variable: `direction`
  --> src/encoding/methods.rs:62:9
   |
62 |     let direction = match direction {
   |         ^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_direction`
warning: unused variable: `pad_token_str`
  --> src/encoding/methods.rs:72:9
   |
72 |     let pad_token_str = match read_optional_utf8(pad_token) {
   |         ^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_pad_token_str`
warning: unused variable: `target_length`
  --> src/encoding/methods.rs:49:5
   |
49 |     target_length: usize,
   |     ^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_target_length`
warning: unused variable: `pad_id`
  --> src/encoding/methods.rs:50:5
   |
50 |     pad_id: u32,
   |     ^^^^^^ help: if this is intentional, prefix it with an underscore: `_pad_id`
warning: unused variable: `pad_type_id`
  --> src/encoding/methods.rs:51:5
   |
51 |     pad_type_id: u32,
   |     ^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_pad_type_id`
warning: unused variable: `direction`
   --> src/encoding/methods.rs:104:9
    |
104 |     let direction = match direction {
    |         ^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_direction`
warning: unused variable: `max_length`
  --> src/encoding/methods.rs:93:5
   |
93 |     max_length: usize,
   |     ^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_max_length`
warning: unused variable: `stride`
  --> src/encoding/methods.rs:94:5
   |
94 |     stride: usize,
   |     ^^^^^^ help: if this is intentional, prefix it with an underscore: `_stride`
warning: unused variable: `sequence_id`
   --> src/encoding/methods.rs:125:5
    |
125 |     sequence_id: usize,
    |     ^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_sequence_id`
warning: unused variable: `word_index`
   --> src/encoding/methods.rs:144:5
    |
144 |     word_index: u32,
    |     ^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_word_index`
warning: unused variable: `sequence_index`
   --> src/encoding/methods.rs:145:5
    |
145 |     sequence_index: usize,
    |     ^^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_sequence_index`
warning: unused variable: `word_index`
   --> src/encoding/methods.rs:166:5
    |
166 |     word_index: u32,
    |     ^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_word_index`
warning: unused variable: `sequence_index`
   --> src/encoding/methods.rs:167:5
    |
167 |     sequence_index: usize,
    |     ^^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_sequence_index`
warning: unused variable: `token_index`
   --> src/encoding/methods.rs:188:5
    |
188 |     token_index: usize,
    |     ^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_token_index`
warning: unused variable: `token_index`
   --> src/encoding/methods.rs:207:5
    |
207 |     token_index: usize,
    |     ^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_token_index`
warning: unused variable: `token_index`
   --> src/encoding/methods.rs:229:5
    |
229 |     token_index: usize,
    |     ^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_token_index`
warning: unused variable: `char_pos`
   --> src/encoding/methods.rs:250:5
    |
250 |     char_pos: usize,
    |     ^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_char_pos`
warning: unused variable: `sequence_index`
   --> src/encoding/methods.rs:251:5
    |
251 |     sequence_index: usize,
    |     ^^^^^^^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_sequence_index`
warning: unused variable: `char_pos`
   --> src/encoding/methods.rs:270:5
    |
270 |     char_pos: usize,
    |     ^^^^^^^^ help: if this is intentional, prefix it with an underscore: `_char_pos`
warning: unused variable: `sequence_index`
   --> src/encoding/methods.rs:271:5
    |
271 |     sequence_index: usize,
