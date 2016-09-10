\ countdown with progress bar
\ (c)copyright 2016 by Gerald Wodni <gerald.wodni@gmail.com>

\ cold

compiletoflash

4 constant pbar-height
8 constant pbar-start
29 constant pbar-end
pbar-end pbar-start - 1+ constant pbar-width
$1F1F1F variable pbar-border
$003F00 variable pbar-fill

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
    \ draw horizontal lines
    pbar-border @ line-color !
    pbar-start pbar-up   pbar-width hline
    pbar-start pbar-down pbar-width hline

    \ draw vertical lines
    pbar-start  pbar-up 1+ pbar-height 2- vline
    pbar-end    pbar-up 1+ pbar-height 2- vline

    \ content width
    pbar-width 2- * 255 /
    
    \ fill
    pbar-fill @ line-color !
    pbar-down pbar-up 1+ do
        pbar-start 1+ i 2 pick hline
    loop drop
    ;

: countdown-step ( max n -- )
    \ change color below n minutes
    dup 5 > if
        dup 10 > if
            $003F00
        else
            $2F2F00
        then
    else
        $3F0000
    then dup text-color ! pbar-fill !
    buffer-off
    0 cur-column !
    \ convert n to string
    dup 0 <# # # #> d-type
    \ show pbar
    255 * swap / pbar
    flush ;

: countdown-thanks ( -- )
    clear
    $3F7F3F text-color !
    d" Thanks!" flush
    5000 ms
    $3F7F3F glitter-color !
    glitter ;

: wait-key ( -- )
    begin key? until
    200 ms key-flush ;

: countdown ( n-minutes -- )
    init-delay
    dup dup countdown-step
    dup 0 do
        .s
        wait-key
        dup dup i - 1- countdown-step
    loop drop
    countdown-thanks ;

10 countdown
