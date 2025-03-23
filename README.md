# Read Me


This is a program that is meant to demonstrate an understanding of multithreading, interprocess communication, and shared memory.
We have two classes a parent and a child. The parent creates 9 children that all communicate back to the parent via named pipes.
Each child is given a pipe name it uses to connect to the parent and acquires a named mutex. Once the child has access to the shared memory/text file, 
it reads the current number in the file, then increments it, then writes the incremented number back to the file.
The child communicates what iteration it's on back to the parent, which then prints the count.
Once the child has completed 5 iterations it releases the mutex letting other children complete their work.

