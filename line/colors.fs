\ Color manglers
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com> 

\ colors are represented as double (i.e $11.22.33), this format is called rgb
\ byte wise colors are alled crgb

compiletoflash

: crgb>rgb ( r g b -- d-rgb )
    swap 8 lshift or \ combine blue and green
    swap ; \ add red as upper word

\ sector size 60 = 360 full circle, 43 = 255 full circle
$FF constant hue-sector
hue-sector 6 * constant hue-max
: >hue ( n -- n-hue-sector-start ) 1-foldable
    hue-sector 60 */ ;

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
    hue-sector /mod >r
    255 * hue-sector /
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
    endcase crgb>rgb ;

cornerstone cold
