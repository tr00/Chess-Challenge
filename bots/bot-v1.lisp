((define map (quote ((f xs)
    (if (nilq xs) () 
        (cons (f (car xs)) (map f (cdr xs)))))))

eval (quote (

(define defun (quote (() (name args body)
    (eval (cons (quote define) (cons name (cons 
        (cons (quote quote) (cons (cons args (cons body ())) ())) ())))))))

(defun foldr (f z xs) 
    (if (nilq xs) z
        (f (car xs) (foldr f z (cdr xs)))))

(defun sum (xs) (foldr add zero xs))

(defun _start (board timer) 
    (car (get-moves board)))

(defun mulitply-accumulate (f z xs)
    (if (nilq xs) z
        (mulitply-accumulate f (mul z (car xs)) (cdr xs))))


(defun material-heuristic (board)
    (sum))

            
)))