\ (c)copyright 2016 by Gerald Wodni
\ Tetris for the 32C3

very-cold


compiletoflash

cols buffer: block-buffer
$000000 variable block-off
$1F0000 variable block-on
$1F0000 variable block-color

10 variable block-x
1  variable block-y

: block-buffer-off ( -- )
    block-buffer cols bounds do 0 i c!  loop ;

\ get mask with nth bit set
: bit ( n -- x )
    1 swap lshift ;

: tetris@ ( x y -- )
    bit swap \ convert column to bit pattern
    block-buffer + c@ \ get column (as byte)
    and 0<> ; \ check if set

\ : tetris@ ( x y -- )
\    ." tetris@ " swap . . cr 1 ;

\ : xy! ( c x y -- )
\     ." xy! "
\     >r swap . . r> . cr  ;

: tetris-row ( x -- )
    rows 8 min 0 do
        dup i tetris@ \ get tetris bit
        if block-on else block-off then \ color
        @ over i xy!
    loop drop ;

\ write tetris buffer on display buffer
: tetris() ( -- )
    buffer-off
    cols 0 do
        i tetris-row
    loop
    flush ;


\ define a new block
: >block ( C: n-4 n-3 n-2 n-1 -- ) <builds
    0
    4 0 do
        4 rshift
        swap 12 lshift or
    loop ,
    does>
        @
    ;

%0000
%1110
%0100
%0000 >block B-T

%0000
%1111
%0000
%0000 >block B-I

%0000
%0111
%0100
%0000 >block B-LR

%0000
%1110
%0010
%0000 >block B-LL

%0000
%0110
%0110
%0000 >block B-V

%0000
%1100
%0110
%0000 >block B-Z

%0000
%0110
%1100
%0000 >block B-S

%1010
%1010
%1110
%1000 >block B-4

create blocks B-T , B-I , B-LR , B-LL , B-V , B-Z , B-S , B-4 ,
8 constant blocks#

: draw-block ( block-pattern -- )
    block-buffer cols + 4 - 4 bounds do
        dup $F and      \ get lowest nibble
        2 lshift i c!   \ set pattern
        4 rshift        \ next nibble
    loop drop ;

: highest-y ( y -- x )
    bit cols 1- begin
        2dup block-buffer + c@ and
        0=
        .s
    while
        1-
    repeat
    ." done"
    .s
    nip ;

: init-tetris
    block-buffer-off
    $FE block-buffer c!
    $FC block-buffer 1+ c!
    $CC block-buffer 2 + c!
    $84 block-buffer 3 + c!
    $80 block-buffer 4 + c!

    B-4 draw-block

    tetris()
    ;

init-tetris
