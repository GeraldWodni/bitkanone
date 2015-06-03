# bitkanone
(c)copyright 2013-2015 by Gerald Wodni

the bitkanone is intended to control WS2812B Led-Strips as displays, it features more stuff around mecrisp-Forth for stellaris

[![Short demo video of the bitkanone](http://img.youtube.com/vi/gMex8-L1E6w/0.jpg)](https://youtu.be/gMex8-L1E6w)

## Authors
mecrisp is a Forth system written by Matthias Koch.
bitkanone itself is written by Gerald Wodni

## License
This Software is Free Software under the terms of GPLv3,
See tools/ directories for their respective licenses.

## Make & Break

Program mecrisp into the lm4f120 using lm4flash (might require sudo):
`$ tools/lm4flash/lm4flash tools/mecrisp/lm4f120/mecrisp-stellaris-lm4f120.bin`

Merge all source-code into one file for an easy upload:
`$ ./merge`

Start a serial-terminal (picoterm)
`$ ./terminal`

Instruct picoterm to upload the merged file:
`<ctrl-a><ctrl-s>bitkanone.fs<return>`

## Words
### Leds direct

```forth
on		( -- )			set all leds to full white
off		( -- )			disable all leds
red		( -- )			set leds to soft color ($1F instead of $FF)
yellow		--//--
green		--//--
cyan		--//--
blue		--//--
magenta		--//--
white		--//--
```

### Buffer

```forth
flush		( -- )			write buffer using the current flush-target (most likely z-flush)
buffer-off	( -- )			clear buffer
buffer!		( u -- )		set all pixels to u
```

### Game of Life (gol)

```forth
game-line	( n -- )
gol-step	( -- )			compute single gol-step
g		( -- )			alias for gol-step ( for easy demonstrion )
game-steps	( n -- )		compute n gol-steps
gol-off		( -- )			clear all life
g!		( n-x n-y -- )		spawn live in cell x y
```

#### gol-patterns

```forth
glider		( -- )			smallest moving object / hacker-symbol 
lwss		( -- )			light-weight-space-ship
gol-line	( n -- )		line with n elements
die-hard	( -- )			small startup colony
acorn 		( -- )			lives forever on infinite grid, not here :/
quadpole	( -- )			static
schick		( -- )
```

### Glitter

```forth
glitter		( -- )			displays a nice glitter-fire-like effect until a key is pressed
glitter-color	( -- a )		color variable, standard value: $FFAF3F
```

### PNM - display images
```forth
pnm-here        ( -- )                  parses an image in pnm-ascci format, some images can be found in /data/images. Just send them with <ctrl-a><ctrl-s>data/images/1701.ppm<return>
```


## tl;dr
```
(sudo) tools/lm4flash/lm4flash tools/mecrisp/lm4f120/mecrisp-stellaris-lm4f120.bin
./merge
./terminal
<c-a><c-s>bitkanone.fs<cr>
```


## Run
On power on the the test pattern will be displayed for one second ( blue-waves with a red dialogal line originating from the top left.
After that the bottom left (1st) Led will turn green. If you press any key from now on within 5 seconds,
it will turn red and you will be greeted by "Human presence deteced".
If no key is received, the ev-demo program is launced and will loop until any key or reset is pressed.

Have fun! ;)
