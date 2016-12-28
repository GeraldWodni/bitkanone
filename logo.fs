: logo ( -- )
	$3F0000 color	\ background
	0 0 pos
	8 8 rect

	$000F0F color	\ colon
	1 1 pos
	2 2 rect
	1 4 pos
	2 2 rect

	5 1 pos		\ semis
	2 2 rect

	5 4 pos		
	6 4 line
	6 5 line

	5 6 pos dot ;

: forth ( -- )
	buffer-off
	logo
	$7F0000 text-color !
	10 column
	d" F"
	13 column
	d" orth" ;

forth flush
