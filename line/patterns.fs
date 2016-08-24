\ Ramps up the brightnes of the first 4 connected Leds, coloring them Red, Yellow, Green and Blue
\ (c)copyright 2016 by Gerald Wodni<gerald.wodni@gmail.com>
compiletoflash

: looper
    0
    begin
        #leds 0 do
            dup
            i $1F * +   \ add phase-offset
            $7F and     \ restrict brightness to 50%
            dup $7F swap - \ invert red
            i rgb-px!   \ store in ith pixel
        loop
        flush 10 ms
        1+
    key? until drop ;
