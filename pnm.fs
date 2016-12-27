\ Parse Portable Anymap (PNM) from input stream and write to buffer
\ Supported Variants: P3 with 24bit colors is supported ( gimp used )
\ (c) copyright 2015 by Gerald Wodni <gerald.wodni@gmail.com>

: not-supported ( -- )
    ." Format not supported" ;

\ read next line, ignore comments
: pnm-query
    begin
        query cr source drop c@ [char] # <>
    until ;

\ parse pnm-header
: pnm-header ( -- x y )
    pnm-query interpret \ read x & y
    query interpret drop \ read maximum value ( and ignore it ! )
    ;

\ currently only P3 is supported
: P1 not-supported ;
: P2 not-supported ;
: P4 not-supported ;
: P5 not-supported ;
: P6 not-supported ;

\ parse Portable Pixmap (ASCII)
: P3 pnm-header
    buffer-off
    0 do \ rows
        dup 0 do \ columns
            query interpret 16 lshift   \ red
            query interpret 8 lshift    \ green
            query interpret             \ blue
            or or                       \ combine
            i j xy!                     \ store
        loop
    loop drop z-flush ;                 \ flush to leds

\ dummy word: read type and 
: pnm-here
    \ read header
    query interpret
    ;

\ 3 pixels (RGB)
\ TODO: will not work with e4thcom.fs
\ pnm-here
\ P3
\ # hallo
\ 3 1
\ 255
\ 255
\ 0
\ 0
\ 1
\ 255
\ 1
\ 2
\ 2
\ 255

