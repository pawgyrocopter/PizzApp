import {Photo} from "./photo";
import {Toping} from "./toping";

export interface Pizza {
  id: number
  name: string
  photoUrl : string
  cost : number
  weight : number
  ingredients : string
  state : State
  stateAsString : string
  topings : Toping[]
  maxCost : number
}

export enum State{
  "Pending",
  "In Progress",
  "Ready",
  "Canceled"
}
