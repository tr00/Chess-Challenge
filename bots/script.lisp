(define defun (quote (() (name args body)
    (cons (quote define) (cons name (cons 
        (cons (quote quote) (cons (cons args (cons body ())) ())) ()))))))

(define material-heuristic (quote ((board)
    (foldr add zero 
        (map (quote ((pair)
            (sum (map (mul-curry (cadr pair)) (car pair)))))
            (zip (get-pieces board)
                (quote (
                        10  30  32  50  90  200
                    -10 -30 -32 -50 -90 -200)))
            ))
    )))