(let lambda (quote (() (args body)
    (eval (cons (quote quote) (cons (cons args (cons body ())) ())) ())))
        
(let _start (lambda (board timer)
    (car (get-moves board)))
    
()))