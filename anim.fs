\ some animations

: mirror ( -- )
    cols 2/ 0 do
        rows 0 do
            j i xy@
            cols 1- j - i xy@
            j i xy!
            cols 1- j - i xy!
        loop
        100 ms flush
    loop ;

: vmirror ( -- )
    rows 2/ 0 do
        cols 0 do
            i j xy@
            i rows 1- j -  xy@
            i j xy!
            i rows 1- j -  xy!
        loop
        200 ms flush
    loop ;

: around ( -- )
    mirror
    3000 ms
    vmirror
    3000 ms
    mirror
    3000 ms
    vmirror ;

