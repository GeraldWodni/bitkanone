\ Color manglers & test cases
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\ colors are represented as double (i.e $11.22.33), this format is called rgb
\ byte wise colors are alled crgb

compiletoflash

: u.2 hex. ; : u.4 hex. ;

\ helpers for handling 3 items on stack
: 3dup >r 2dup r@ -rot r> ;
: 3drop 2drop drop ;

: crgb>rgb ( r g b -- d-rgb )
    swap 8 lshift or \ combine blue and green
    swap ; \ add red as upper word

\ sector size 60 = 360 full circle, 43 = 255 full circle
$FF constant hsv-sector

\ p, q, t and V as color parts 
: hsv-p ( s v f -- p )
    drop >r >r 255 r> - r> * 8 rshift ;

: hsv-q ( s v f -- q )
    rot * 8 rshift 255 swap - * 8 rshift ;

: hsv-t ( s v f -- t )
    rot swap \ v s f
    255 swap - * 8 rshift
    255 swap - * 8 rshift ;

: hsv-V ( s v f -- V )
    drop nip ;

\ sector and remainder
: hsv-f-hi ( h -- f hi )
    hsv-sector /mod >r
    255 * hsv-sector /
    r>
    ;

\ hsv to rgb conversion (byte-based)
: hsv>rgb ( h s v -- d-rgb )
    rot hsv-f-hi \ s v f hi
    case \ s v f
        1 of
            \ ." Sector1" cr
            \ q V p
            3dup hsv-p >r
            3dup hsv-V >r
            hsv-q r> r>
        endof
        2 of
            \ ." Sector2" cr
            \ p V t
            3dup hsv-t >r
            3dup hsv-V >r
            hsv-p r> r>
        endof
        3 of
            \ ." Sector3" cr
            \ p q V
            3dup hsv-V >r
            3dup hsv-q >r
            hsv-p r> r>
        endof
        4 of
            \ ." Sector4" cr
            \ t p V
            3dup hsv-V >r
            3dup hsv-p >r
            hsv-t r> r>
        endof
        5 of
            \ ." Sector5" cr
            \ V p q
            3dup hsv-q >r
            3dup hsv-p >r
            hsv-V r> r>
        endof
            \ ." Sector0,6" cr
        drop \ V t p
            3dup hsv-p >r
            3dup hsv-t >r
            hsv-V r> r>
        0
    endcase  ;

: h dup u.4 space space $FF $FF hsv>rgb >r >r u.2 space r> r> swap u.2 space u.2 cr ;
: ht 3 + dup 5 - do i h loop ;
: htt
    ." red" cr
    2 ht
    ." yellow" cr
    $FF ht
    ." green" cr
    $1FE ht
    ." cyan" cr
    $2FD ht
    ." blue" cr
    $3FC ht
    ." magenta" cr
    $4FB ht ;
htt

: col= ( n-r1 n-g1 n-b1 n-r2 n-g2 n-b2 -- f ) 
    >r rot r> = >r
    rot = r> and 
    -rot = and ;

: col. ( r g b -- )
    rot hex. swap hex. hex. ;

: col-check ( angle r1 g1 b1 r2 g2 b2 -- )
    col= 0= if
        ." ERROR: angle " hex. cr
    else
        drop
    then ;
    
: tester
    ." -=Test Colors =-" cr
    $100 0 do
        i dup $FF $FF hsv>rgb
        $FF i 0 col-check

        i $FF + dup $FF $FF hsv>rgb
        $FF i - $FF 0 col-check

        i $1FE + dup $FF $FF hsv>rgb
        0 $FF i col-check

        i $2FD + dup $FF $FF hsv>rgb
        0 $FF i - $FF col-check

        i $3FC + dup $FF $FF hsv>rgb
        i 0 $FF col-check

        i $4FB + dup $FF $FF hsv>rgb
        $FF 0 $FF i - col-check
    loop
    ." -=Test Black=-" cr
    $FF 6 * 0 do
        i $FF $0  hsv>rgb $0 $0 $0 col= 0= if
            ." Black error on " i . cr
        then
    loop
    ;
tester

\ problem: 256 works correctly on PC, check for overflow in math!
