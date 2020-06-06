[<FsCheck.Xunit.Properties(Arbitrary=[| typeof<ByteBasedEdges.MachineGenerators> |], MaxTest = 2000, EndSize = 10)>]
module FiniteStateMachine.Accept

open Slp.Fsm
open FsCheck.Xunit

let accepts = FiniteStateMachine.accepts ByteBasedEdges.evaluator

[<Property>]
let ``Initial machine`` (input:list<byte>) =
    let accepts =
        GenericNode.initialMachine ()
        |> accepts input

    accepts = (input |> List.isEmpty)

[<Property>]
let ``Machine + any byte`` (originalMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let updatedMachine =
        originalMachine |> GenericNode.appendEdge AnyByte

    if input |> List.isEmpty then
        updatedMachine 
        |> accepts input
        |> not
    else
        let prev = input |> List.rev |> List.tail |> List.rev

        (accepts input updatedMachine) = (accepts prev originalMachine)

[<Property>]
let ``Machine + exact byte`` (originalMachine: ByteBasedEdges.ByteBasedFsm) (byte: byte) (input: byte list) =
    let updatedMachine =
        originalMachine |> GenericNode.appendEdge (ExactByte byte)

    if input |> List.isEmpty then
        updatedMachine
        |> accepts input
        |> not
    else
        let revInput = input |> List.rev
        let last = revInput |> List.head
        let prev = revInput |> List.tail |> List.rev

        (accepts input updatedMachine) = ((accepts prev originalMachine) && last = byte)

[<Property>]
let ``Machine append`` (leftMachine: ByteBasedEdges.ByteBasedFsm) (rightMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let completeMachine = leftMachine |> FiniteStateMachineBuilder.appendMachine rightMachine

    let rec orMatches acc inp =
        match inp with
        | [] -> (leftMachine |> accepts (acc |> List.rev)) && (rightMachine |> accepts [])
        | (x :: xs) as i ->
            if (leftMachine |> accepts (acc |> List.rev)) && (rightMachine |> accepts i) then
                true
            else
                orMatches (x :: acc) xs

    (orMatches [] input) = (completeMachine |> accepts input)

[<Property(Replay="212548222,296752373")>]
let ``Machine optional`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> FiniteStateMachineBuilder.optional

    (machine |> accepts input) = (orMachine |> accepts input || input |> List.isEmpty)

let private availableRepetitions input machine =
    let rec containsRepeats acc inp count =
        if count < 0 then
            false
        else
            match acc, inp with
            | [], [] ->
                if count = 0 then true
                else machine |> accepts []
            | _, [] ->
                if machine |> accepts (acc |> List.rev) then
                    containsRepeats [] [] (count - 1)
                else
                    false
            | _, x::xs ->
                (machine |> accepts (acc |> List.rev) && containsRepeats [] inp (count - 1)) || (containsRepeats (x::acc) xs count)

    Seq.init 20 id
    |> Seq.filter (containsRepeats [] input)

[<Property(Replay="1454438580,296752372")>]
let ``Machine infinite repeat`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> FiniteStateMachineBuilder.infiniteRepeat
    (machine |> accepts input) = (orMachine |> availableRepetitions input |> Seq.isEmpty |> not)

[<Property>]
let ``Machine at least one repeat`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
   let machine = orMachine |> GenericNode.atLeastOneRepeatMachine
   (machine |> accepts input) = (orMachine |> availableRepetitions input |> Seq.filter (fun x -> x > 0) |> Seq.isEmpty |> not)

[<Property>]
let ``Machine fixed repeat (3)`` (orMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = orMachine |> GenericNode.repeatMachine 3
    (machine |> accepts input) = (orMachine |> availableRepetitions input |> Seq.filter (fun x -> x = 3) |> Seq.isEmpty |> not)

[<Property(Replay="1494769171,296752372")>]
let ``Machine choice`` (leftMachine: ByteBasedEdges.ByteBasedFsm) (rightMachine: ByteBasedEdges.ByteBasedFsm) (input: byte list) =
    let machine = [ leftMachine; rightMachine ] |> GenericNode.choiceMachine
    (machine |> accepts input) = ((leftMachine |> accepts input) || (rightMachine |> accepts input))
