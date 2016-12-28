\ Turtle graphics for the bitkanone
\ (c)copyright 2016 by Gerald Wodni<gerald.wodni@gmail.com>

compiletoflash

\ turtle position and color
0 variable tx
0 variable ty
$202020 variable tc
\ turtle offsets
0 variable tox
0 variable toy

\ set turtle position
: pos ( x y -- )
    ty ! tx ! ;

\ set turtle color
: color ( d-rgb -- )
    tc ! ;

\ draw dot
: dot ( -- )
    tc @ tx @ tox @ +
         ty @ toy @ + xy! ;

\ order 2 number descending, increment higher
: desc1+ ( u1 u2 -- u3 u4 )
    2dup < if swap then swap 1+ swap ;

\ draw orthogonal line
: line ( to-x to-y -- )
    2dup \ save end point
    dup ty @ = if
        \ horizontal line
        drop
        tx @ desc1+ ?do
            i tx ! dot
        loop
    else
        \ vertical line
        nip
        ty @ desc1+ ?do
            i ty ! dot
        loop
    then pos ; \ save new position

: rect ( n-width n-height -- )
    >r tx @ swap bounds \ horizontal bounds
    ty @ r> bounds do   \ vertical loop
        2dup do
            i j pos dot \ draw
        loop
    loop 2drop ;

\ https://en.wikipedia.org/wiki/In-place_matrix_transposition
\ transpose buffer
: transpose ( -- )
    cols 1- 0 do    \ for j = 0 to N - 2
        cols i 1+ do \ for i = j + 1 to N - 1
            j i xy@   \ swap A(j,i) with A(i,j)
            i j xy@
            j i xy!
            i j xy!
        loop
    loop ;


\ mirror buffer along y-axis
: mirror ( -- )
    cols 0 do       \ row (j)
        cols 2/ 0 do \ column (i)
            i  j xy@
            cols 1- i - j xy@
            i  j xy!
            cols 1- i - j xy!
        loop
    loop ;

\ http://stackoverflow.com/questions/42519/how-do-you-rotate-a-two-dimensional-array
\ rotate clock-wise
: cw ( -- )
    transpose
    mirror ;

: test-img ( -- )
    leds 0 do
        leds i - 0 i led-n!   \ blue background
    loop
    rows 1 do
        $001000 i i xy! \ green diagonal
    loop
    $3F0000 0 led-n!   \ start-px
    $1F1F00 4 led-n! ; \ right-top-px
