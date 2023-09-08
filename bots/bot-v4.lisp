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

(let negamax-inner-helper (lambda (board moves lower upper depths)
    (if (nilq moves) lower

        (let move (car moves)
            (let score (sub zero 
                (negamax-inner
                    (make-move board move) 
                    (sub zero upper) 
                    (sub zero lower)
                    (sub depths one)))
                    
                    (if (ge score upper) (car (cons upper (cons (undo-move board move) ())))
                        (negamax-inner-helper 
                            (undo-move board move)
                            (cdr moves)
                            (if (lt lower score) score lower)
                            upper depths))))))

(let negamax-inner (lambda (board lower upper depths)
    (if (eq depths zero) (material-heuristic board)
        (negamax-inner-helper board (get-moves board) lower upper depths)))

(let negamax-outer-helper (lambda (board moves lower upper depths best)
    (if (nilq moves) best
        (let move (car moves)
            (let score (sub zero 
                (negamax-inner
                    (make-move board move) 
                    (sub zero upper) 
                    (sub zero lower)
                    (sub depths one)))
                    
                    (if (ge score upper) (car (cons best (cons (undo-move board move) ())))
                        (negamax-outer-helper 
                            (undo-move board move)
                            (cdr moves)
                            (if (lt lower score) score lower)
                            upper depths
                            (if (lt lower score) move best)))))))

(let negamax-outer (lambda (board depths)
    (negamax-outer-helper board (get-moves board) -9223372036854775806 9223372036854775806 depths ()))


(let _start (lambda (board timer)
    (negamax-outer board 3))
    
()))))))))))))