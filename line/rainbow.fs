compiletoflash

0 variable rstep

: rainbow-step
    rstep @ 
    dup . ." : "
    #leds 0 do
        dup 360 #leds / i * +
        360 mod
        dup .
        255 255 hsv>rgb i rgb-px!
    loop 1+ 360 mod rstep ! cr ;
    
rainbow-step

: looper ( -- )
    init-spi
    begin
        rainbow-step
        flush
        10 ms
    key? until ;
 
looper
