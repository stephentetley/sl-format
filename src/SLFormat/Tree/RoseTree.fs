﻿// Copyright (c) Stephen Tetley 2019
// License: BSD 3 Clause

// Acknowledgment
// ==============
// This is an implementation of the printing code
// from Haskell's Base Data.Tree module, the code has
// been CPS transformed.

namespace SLFormat


module RoseTree = 
    
    open System

    
    
    /// Splits on Environment.NewLine
    let private toLines (source:string) : string list = 
        source.Split(separator=[| Environment.NewLine |], options=StringSplitOptions.None) |> Array.toList
    
    /// Joins with Environment.NewLine
    let private fromLines (source:string list) : string = 
        String.concat Environment.NewLine source


    type RoseTree<'a> = RoseTree of 'a * RoseTree<'a> list

    let private shift (first : string) 
                      (other : string) 
                      (source : string list) 
                      (continuation : string list -> string list) : string list = 
        let rec work1 xs = 
            match xs with
            | [] -> continuation []
            | s1 :: ss -> 
                let a1 = first + s1
                work2 ss (fun ac -> continuation (a1 :: ac))
        and work2 xs cont = 
            match xs with
            | [] -> cont []
            | s1 :: ss -> 
                let a1 = other + s1
                work2 ss (fun ac -> cont (a1 :: ac))
        work1 source



            


    let private drawTree1 (source : RoseTree<string>) : string list = 
        let rec draw1 (RoseTree(label, kids) : RoseTree<string>) (cont : string list -> string list) = 
            let xs = toLines label 
            drawKids kids (fun ac -> cont (xs @ ac))
        and drawKids xs (cont : string list -> string list) = 
            match xs with
            | [] -> cont []
            | [t1] -> 
                draw1 t1                    (fun ac1 -> 
                shift "`- " "   " ac1       (fun ac2 -> 
                cont ("|" :: ac2)))
            | t1 :: ts -> 
                draw1 t1                        (fun ac1 ->
                shift "+- " "|  " ac1           (fun ac2 -> 
                drawKids ts                     (fun ac3 -> 
                cont ("|" :: (ac2 @ ac3)))))
        draw1 source id


    let drawTree (tree : RoseTree<string>) : string = 
        fromLines <| drawTree1 tree

    let mapTree (mapper : 'a -> 'b) (tree : RoseTree<'a>) : RoseTree<'b> = 
        let rec map1 (RoseTree(a, kids)) (cont :RoseTree<'b> -> RoseTree<'b>) = 
            let label = mapper a
            mapKids kids (fun ac -> 
            cont (RoseTree(label, ac)))
        and mapKids xs (cont : RoseTree<'b> list -> RoseTree<'b>) = 
            match xs with 
            | [] -> cont []
            | t1 :: ts -> 
                map1 t1 (fun a1 -> 
                mapKids ts (fun ac -> 
                cont (a1 :: ac)))
        map1 tree id
