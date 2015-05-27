\ progress bar
\ (c)copyright 2015 by Gerald Wodni <gerald.wodni@gmail.com>

gol


compiletoflash

4 constant pbar-height
$1F1F1F variable pbar-border
$3F0000 variable pbar-fill

rows 2/ pbar-height 2/ - constant pbar-up
pbar-up pbar-height + 1- constant pbar-down

$1F0000 variable line-color

\ draw a horizontal line
: hline ( n-x n-y n-len -- )
    rot swap bounds ?do
        line-color @ i 2 pick xy!
    loop drop ;

\ draw a vertical line
: vline ( n-x n-y n-len -- )
    bounds ?do
        line-color @ over i xy!
    loop drop ;

\ draw progressbar value ranges from 0 to 255
: pbar ( n-value -- )
    buffer-off
    \ draw horizontal lines
    pbar-border @ line-color !
    0 pbar-up   cols hline
    0 pbar-down cols hline

    \ draw vertical lines
    0 pbar-up 1+ pbar-height 2- vline
    cols 1- pbar-up 1+ pbar-height 2- vline

    \ content width
    cols 2- * 255 /
    \ fill
    pbar-fill @ line-color !
    pbar-down pbar-up 1+ do
        1 i 2 pick hline
    loop drop

    z-flush ;

: progress ( -- )
    buffer-off
    $3F0000 pbar-fill !
    256 0 do
        i pbar
        10 ms
    loop
    \ finish with green pbar
    $003F00 pbar-fill !
    255 pbar
    ;

progress
