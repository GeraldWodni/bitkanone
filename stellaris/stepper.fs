\ (c)copyright 2014 by Gerald Wodni
\ Control a 28BYJ-48 steper via a ULN2003 driver 

2000 variable speed	\ delay in us between steps
0 variable cur-step	\ iterates through the 8 patterns
1 variable direction	\ can be 1 or -1

\ setup port
: init-stepper ( -- )
	init-delay
	$F0 PORTA_DEN  bis!
	$F0 PORTA_DATA bic!
	$F0 PORTA_DIR  bis! ;

\ driver higher nibble of the port without touching other bits
: >port ( x -- )
	$F and 4 lshift
	dup invert PORTA_DATA bic! 	\ clear former 1s
	PORTA_DATA bis! ; 		\ set new 1s

\ set pattern
: >motor ( n -- )
	case
		0 of $9 endof
		1 of $1 endof
		2 of $3 endof
		3 of $2 endof
		4 of $6 endof
		5 of $4 endof
		6 of $C endof
		7 of $8 endof
		0
	endcase >port ;

\ perform 1 step
: step ( -- )
	cur-step @ direction @ + $7 and dup >motor cur-step ! ;

\ perform n steps
: steps ( n-steps -- )
	0 do speed @ us step loop ;

init-stepper
1000 steps
