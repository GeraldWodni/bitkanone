\ main include file for e4thcom
\ use `#include e4thcom.fs`

\ base functions
compiletoflash
#include utils.fs
#include pll.fs
#include delay.fs
#include spi.fs

#include ws2812.fs
#include random.fs
#include turtle.fs

create 8px
#include data/8px.fs
create 8px-cond
#include data/8px-cond.fs
cornerstone raw

\ additional functionality for easy demoing, with extended cold
#include text.fs
#include markup.fs
#include gol.fs
#include glitter.fs
#include pnm.fs

cornerstone very-cold

#include anim.fs
#include logo.fs
#include 33c3.fs

cornerstone cold

init
