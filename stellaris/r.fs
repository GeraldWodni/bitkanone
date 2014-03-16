rewind-w

compiletoflash
: row-bounds ( n -- )
	dup .
	row-size *      \ row offset
	led-buffer +    \ total offset
	row-size bounds ;

\ write line
: row>rgb ( a-addr-start a-addr-end -- )
	do
		i @ >rgb
	4 +loop ;

\ write reversed
: -row>rgb ( a-addr-start a-addr-end -- )
	swap
	4 -
	do
		i @ >rgb
	-4 +loop ;

\ takes about 25ms
: rinit
	hex
	initi
	off
	\ calculate bounds first
	8 0 do
		i .
		i row-bounds swap . . cr
	loop
	off
	8 0 do
		i row-bounds
	loop
	\ send in oposing directions
	4 0 do
		-row>rgb
		row>rgb
	loop
	;

rinit
