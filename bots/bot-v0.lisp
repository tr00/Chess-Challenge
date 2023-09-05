(
    (define map-eval (quote ((xs)
        (if (nilq xs) () 
            (cons (eval (car xs)) (map-eval (cdr xs)))))))

(quote (
    (define _start (quote ((board timer) (car (gen-moves board)))))
            
)))