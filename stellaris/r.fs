rewind-w

compiletoflash

: row-bounds ( n -- )
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

: rflush ( -- )
	0 7 do
		i row-bounds
		i 1 and if
			-row>rgb
		else
			row>rgb
		then
	-1 +loop
	;

\ takes about 10ms
: rinit
	hex
	initi
	off
	\ calculate bounds first
	8 0 do
		i .
		i row-bounds swap . . cr
	loop
	rflush
	;

rinit
