((define map (quote ((f xs)
    (if (nilq xs) () 
        (cons (f (car xs)) (map f (cdr xs)))))))

eval (quote (

    (define _start (quote ((board timer) (car (gen-moves board)))))

    (define negamax-inner-helper (quote ((board moves lower upper depths))
        (if (nilq moves) lower
            (let move (car moves)
                (let score (sub zero (negamax-inner (make-move board move) 
                        (sub zero upper) (sub zero lower) (sub depths one)))

                    (if (ge score upper) upper
                        (negamax-inner-helper (undo-move board move) (cdr moves) 
                            (if (lt lower score) score lower) upper depths)))))))

    (define negamax-inner (quote ((board lower upper depths)
        (if (eq depths zero)
            (qiescence board lower upper)
            (negamax-inner-helper board (gen-moves board) lower upper depths)))))

    (define negamax-outer-helper (quote ((board moves upper depths best)
        (if (nilq moves) best
            (let move (car moves)
                (let score (sub 0 (negamax-inner 
                        (make-move board move) (sub 0 upper) limit (sub depths 1)))

                    (if (lt upper score)
                        (negamax-outer-helper (undo-move board move) (cdr moves) score depths move)
                        (negamax-outer-helper (undo-move board move) (cdr moves) upper depths best)

                    )
                ))
            )
        )))

    (define negamax-outer (quote ((board depths)
        )))

    (define iterative-deepening (quote (board timer depths best-move)
        (if (time-is-up timer) best-move
            (iterative-deepening board timer (add depths 1) 
                (negamax-outer board depths)))))


            
)))