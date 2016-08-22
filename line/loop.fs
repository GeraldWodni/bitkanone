\ Ramps up the brightnes of the first 4 connected Leds, coloring them Red, Yellow, Green and Blue
\ (c)copyright 2016 by Gerald Wodni<gerald.wodni@gmail.com>

cold


compiletoflash

: green-loop ( -- )
    0
    begin

        0   >wsi
        dup >wsi
        0   >wsi

        dup >wsi
        dup >wsi
        0   >wsi

        dup >wsi
        0   >wsi
        0   >wsi

        0   >wsi
        0   >wsi
        dup >wsi

        10 ms
        1+
    key? until drop ;
init-spi
green-loop
