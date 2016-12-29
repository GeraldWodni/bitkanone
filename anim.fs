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
