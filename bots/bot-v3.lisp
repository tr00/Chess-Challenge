(let lambda (quote (() (args body)
    (eval (cons (quote quote) (cons (cons args (cons body ())) ())) ())))
        
(let foldr (lambda (f z xs)
    (if (nilq xs) z
        (f (car xs) (foldr f z (cdr xs)))))
   
(let map (lambda (f xs)
    (foldr cons () xs))

(let length (lambda (xs)
    (if (nilq xs) zero
        (add one (length (cdr xs)))))

(let piece-weights (quote 
    (100 320 330 500 900 20000 -100 -320 -330 -500 -900 -20000))

(let material-heuristic-helper (lambda (pieces weights)
    (if (nilq pieces) zero
        (add (mul (length (car pieces)) (car weights))
            (material-heuristic-helper (cdr pieces) (cdr weights)))))

(let material-heuristic (lambda (board)
    (mul (material-heuristic-helper (get-pieces board) piece-weights)
        (if (side-to-move board) one (sub zero one))))

(let _start (lambda (board timer)
    (car (get-moves board)))
    
()))))))))