# UK Parliament - Software Engineering

## Introduction

Thanks for taking our code test. We'd like you to implement an API for a fictional room booking system. 

We have put together a starter solution for you. This starter solution includes:
* A .net core API
* Swagger for API docs
* Associated assemblies
* Entity framework with an in-memory provided attached
* The beginnings of a data model. It is important to note **this model is incomplete, you will need to extend it**

You should be able to run this project out of the box in kestrel. 

## Submitting your test
* After you have complete this test please host your solution on GitHub (or another git based hosting platform)
* Provide us with a link so we can clone your solution

## Task
We would like you to do the following
### People
* Write a series of endpoints to manage people
    * Add people        
    * Update people
    * Search for and retrieve specific people
    * Delete people, behaving appropriately if a person is in use
### Rooms
* Write a series of endpoints to manage rooms
    * Add rooms
        * Rooms must have a unique name
    * Update rooms    
    * Search for rooms
    * Delete rooms, optionally shifting all bookings to another specified room
### Bookings
* Write a series of endpoints to manage room bookings
    * Add a room booking 
        * A room booking can be for a variable length of time, but it can be at maximum 1 hour long
    * Remove a room booking
    * See what rooms are available for a given time period

