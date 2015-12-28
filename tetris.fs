\ (c)copyright 2015 by Gerald Wodni
\ Tetris for the 32C3

\ DECT Handy: 8433

very-cold
compiletoflash

16 buffer: block-buffer
$000000 variable block-off
$1F0000 variable block-on
$1F0000 variable block-color

10 variable block-x
1  variable block-y

: dump ( c-addr n -- )
    cr
    bounds do
        i hex. ."  "
        i c@ hex. cr
    loop ;

: block-buffer.
    block-buffer 16 dump ;

: block-xy@ ( -- x y )
    block-x @ block-y @ ;

: block-xy! ( -- x y )
    block-y ! block-x ! ;

: block-xy@? ( x y -- f )
    4 * + block-buffer + c@ ;

: block-find-y ( n-start n-offset -- n-y )
    >r
    begin
        0
        4 0 do
            over i swap block-xy@? or
        loop
    0= while
        r@ +
    repeat rdrop ;

: block-first-y ( -- n-y )
    0  1 block-find-y ;

: block-last-y ( -- n-y )
    3 -1 block-find-y ;
    

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
%0010 >block B-4

create blocks B-T , B-I , B-LR , B-LL , B-V , B-Z , B-S , B-4 ,

\ load block into block-buffer
: load-block ( x -- )
    16 0 do
        dup $8000 and if $FF else $00 then
        i block-buffer + c!
        1 lshift
    loop drop ;

: draw-block ( x y -- )
    4 0 do
        4 0 do
            i j block-xy@? if \ color
                over i + \ x
                over j + \ y
                block-color @
                -rot xy!
            then
        loop
    loop 2drop ;

: @col!-draw-block
    @ block-color ! block-xy@ draw-block ;

: draw-off-block
    block-off @col!-draw-block ;

: draw-on-block
    block-on  @col!-draw-block ;

\ move block down and redraw it
: block+! ( n addr -- )
    draw-off-block +! draw-on-block ;

: block-down ( -- )
    -1 block-x block+! ;

: block-left ( -- )
    -1 block-y block+! ;

: block-right ( -- )
     1 block-y block+! ;

: block-down? ( -- f )
    \ always stop at the bottom
    block-x @ 0= if
        false
    else
        \ space below occupied?
        true
    then ;

: game-over? ( -- f )
    false ;

\ load random block
: next-block ( -- )
    random $FFFFFF and block-on !
    random 7 and cells blocks + @ load-block ;

: down ( -- )
    block-down? if
        block-down
    else
        game-over? if
            ." game over" cr
        else
            27 2 block-xy!
            next-block
        then
    then flush ;

: left ( -- )
    
    block-left ;

: right ( -- )
    block-right ;
    

: block-autopilot ( -- )
    begin
        down
        500 ms
        key? if \ move
            key case
                [CHAR] w of left  endof
                [CHAR] s of right endof
                dup
            endcase
            27 = \ break on escape
        else
            false
        then
    until ;
    

: init-tetris 
    next-block block-xy@ draw-block flush
    ;
    \ block-autopilot ;

init-tetris
