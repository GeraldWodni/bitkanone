\ XOR-Shift pseudo random number generator
\ (c) copyright 2015 by Gerald Wodni <gerald.wodni@gmail.com>

42137 variable seed
137 variable c-seed

\ xor-shift, http://www.arklyffe.com/main/2010/08/29/xorshift-pseudorandom-number-generator/
: random ( -- x )
    seed @      \ get current seed
    dup 13 lshift xor   \ apply xor shift
    dup  9 rshift xor
    dup  7 lshift xor
    dup seed !  ;       \ store for next seed

\ 8-bit random generator
: c-random ( -- c )
    c-seed @
    dup 7 lshift xor
    dup 5 rshift xor
    dup 3 lshift xor
    dup c-seed ! ;

\ generates matching bitmask for n
: match-bits ( u -- x )
    0 begin
        1 lshift 1 or   \ add bit
        swap 2/ swap    \ right-shift input
        over 0=         \ input zero?
    until nip ;         \ drop input

\ random lower enclosing power of 2
: random-max2 ( u-max^2 -- u )
    match-bits random and ;

\ random lower enclosing power of 2
: c-random-max2 ( u-max^2 -- u )
    match-bits c-random and ;

\ random lower u-max
: random-max ( u-max -- u )
    >r
    begin
        r@ random-max2 dup r@ >=
    while
        drop
    repeat rdrop ;

\ random lower u-max
: c-random-max ( u-max -- u )
    >r
    begin
        r@ c-random-max2 dup r@ >=
    while
        drop
    repeat rdrop ;

: rand ( n -- )
    0 do 17 c-random-max . cr loop ;

: random-xy ( -- x y )
    cols c-random-max
    rows c-random-max ;
