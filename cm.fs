\ ColorMander - Tiny Dungeon game for the bitkanone
\ (c)copyright Gerald Wodni <gerald.wodni@gmail.com>

\ quick howto: move with HJKL
pb

compiletoflash

0           variable player-x
rows 2/     variable player-y
player-x @  variable player-old-x
player-y @  variable player-old-y

\ materials
$0F3F03 constant solid
$3F3F00 constant player-color
$000004 constant player-trail

\ set current material
: mat! ( color -- )
    line-color ! ;

: player@ ( -- x y )
    player-x @ player-y @ ;

: player! ( x y -- )
    player-y !  player-x ! ;

\ add two vec2s
: vec2+ ( x1 y1 x2 y2 -- x3 y3 )
    rot + >r + r> ;

\ assure display bounds
: vec2-bounds ( x1 y1 -- x2 y2 )
    0 rows 1- limits >r
    0 cols 1- limits r> ;

\ move player
: cm-move ( x y -- )
    \ clear and save old position
    player-trail player@ 2>r 2r@ xy!

    \ new position within limits
    2r@ vec2+ vec2-bounds

    2r> 2swap
    \ old-x old-y x y

    \ 2dup xy@ solid = if
    \     2drop
    \ else
    \     2nip
    \ then

    2dup xy@ case
        \ solid: keep position
        solid of 2drop endof
        >r 2nip r>  \ normal movement
    endcase

    \ 2r>



    \ draw new position
    2>r player-color 2r@ xy!
    \ update position
    2r> player!

    z-flush ;

\ set position
: cm-position ( x y -- )
    player-y ! player-x !
    0 0 cm-move ;

\ levels
: level1
    solid mat!
    0 7 5 hline
    4 3 4 vline
    5 3 4 hline
    8 3 5 vline
    0 0 cm-position ;

\ main game
: cm ( -- )
    buffer-off
    level1
    begin
        key dup 27 <> \ stop game loop on escape
    while
        case
            [char] h of -1  0 cm-move endof
            [char] l of  1  0 cm-move endof
            [char] k of  0 -1 cm-move endof
            [char] j of  0  1 cm-move endof
        endcase
    repeat 
    drop off ;
cm
