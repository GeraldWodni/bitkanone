\ (c)copyright 2014 by Gerald Wodni
\ Implementation of Conways game of live
\ uses the highest byte of the color-buffer (during computation)

cold


compiletoflash

\ colors
$0F0F00 constant gol-alive
$000002 constant gol-dead

\ fast modulo which assumes nominator is in [-denominator denominator*2)
: wrap ( n-nominator n-denominator -- n-remainder )
	over 0< if
		+            \ smaller than 0, just add denominator
	else
		2dup < if    \ lower than denominator, okay
			drop
		else
			-    \ bigger, subtract denominator
		then
	then ;

\ calculate cartesian coordinates with overflow protection
: xy ( n-x n-y -- n-index )
	rows wrap               \ ensure bounds
	cols * 			\ row-offset
	swap cols wrap   	\ ensure bounds
	+ ;

\ get color
: xy@ ( n-x n-y -- x-color )
	xy led-n @ ;

\ set color
: xy! ( x-xolor n-x n-y -- )
	xy led-n! ;

\ currently alive
: alive@ ( n-x n-y -- f )
	xy@ $FFFFFF and gol-alive = ;

\ count living neighbors
0 variable cur-neighbors
: neighbors ( n-x n-y -- n-alive )
	2dup alive@ if -1 else 0 then cur-neighbors ! 		\ subract the cell itself 
	1- 3 bounds do 						\ walk rows
		dup 1- 3 bounds do 				\ walk columns
			i j alive@ if 1 cur-neighbors +! then 	\ increment counter if alive
		loop 
	loop drop cur-neighbors @ ;

\ check next iteration live value
: cell-alive ( n-x n-y -- f )
	2dup neighbors >r
	alive@ if
		r@ 2 = r> 3 = or 	\ cell alive, keep alive?
	else
		r> 3 = 			\ cell dead, spawn live?
	then ;

\ set msb if cell is alive in next iteration
: cell-step ( n-x n-y -- )
	2dup xy led-n -rot cell-alive if
		$80000000 swap bis!
	else
		$80000000 swap bic!
	then ;

\ perform complete gol-step
: gol-step ( -- )
	\ calculate next iteration
	rows 0 do
		cols 0 do
			i j cell-step
		loop
	loop

	\ paint next iteration
	rows 0 do
		cols 0 do
			i j xy@ $80000000 and if
				gol-alive
			else
				gol-dead
			then
			i j xy!
		loop
	loop
	z-flush ;

: gol-steps ( n -- )
	0 do gol-step 300 ms loop ;

\ kill all cells
: gol-off 
	buffer-off z-flush ;

\ set gol-cell
: g! ( n-x n-y -- )
	gol-alive -rot xy! ;

: glider ( -- )
	buffer-off
	         15 2 g!
	                  16 3 g!
	14 4 g!  15 4 g!  16 4 g!
	z-flush ;

: lwss ( -- )
	buffer-off
	13 2 g!                    16 2 g!
	                                    17 3 g!
	13 4 g!                             17 4 g!
	         14 5 g!  15 5 g!  16 5 g!  17 5 g!
	z-flush ;

: gol-line ( n -- )
	buffer-off
	15 over 2/ - swap
	bounds do
		i 3 g!
	loop 
	z-flush ;

: die-hard ( -- )
	buffer-off
	18 2 g!
	17 4 g!
	18 4 g!
	19 4 g!

	12 3 g!
	13 3 g!
	13 4 g!
	z-flush ;

: acorn ( -- )
	buffer-off
	17 3 g!
	18 4 g!
	19 4 g!
	20 4 g!

	15 2 g!
	15 4 g!
	14 4 g!
	z-flush ;

: init-gol
	init-delay
	init-ws
	10 gol-line
	20 gol-steps
	;

init-gol

