(let lambda (quote (() (args body)
    (eval (cons (quote quote) (cons (cons args (cons body ())) ())) ())))
        
(let foldr (lambda (f z xs)
    (if (nilq xs) z
        (f (car xs) (foldr f z (cdr xs)))))

(let foldr2 (lambda (f z xs ys)
    (if (nilq xs) z
        (f (car xs) (car ys)
            (foldr2 f z (cdr xs) (cdr ys)))))
   
(let map (lambda (f xs)
    (foldr cons () xs))

(let material-heuristic (lambda (board)
    (foldr2 (lambda (p w s) (add s
            (map (lambda (x) (mul x w)) p))) 
        zero (get-pieces board) (quote (
             100  320  350  500  900  20000
            -100 -320 -350 -500 -900 -20000))))

(let _start (lambda (board timer)
    (car (get-moves board)))
    
    
()))))))