// Simple_Game.cpp : This file contains the 'main' function. Program execution begins and ends there.
// ctrl F5 to run
//#include "stdafx.h"
#include <iostream>
#include <windows.h>

struct Player
{
    int level;
    int health;
    
    // new yell function
    void yell()
    {
        std::cout << "Raaghgghh!!!!" << std::endl;

    }
};

struct Game
{
    Player* player;
    int num;
    void point() //added this, this can only be called in an object fucntion?
    {
        std::cout << this << std::endl;
    }
};

Game* pGame;
int main()
{
    pGame = new Game;
    pGame->player = new Player;
    pGame->player->level = 1;
    pGame->player->health = 100;

    INPUT_RECORD event;
    HANDLE hStdIn = GetStdHandle(STD_INPUT_HANDLE);
    DWORD count;

    std::cout << "Welcome to Bloog's Quest!" << std::endl;
    pGame->point(); //added this, calls point() to print game object pointer
    std::cout << "Player is level 1 and has 100 health." << std::endl << std::endl;
    

    while (true)
    {
        // handle user input
        if (WaitForSingleObject(hStdIn, 0) == WAIT_OBJECT_0)
        {
            ReadConsoleInput(hStdIn, &event, 1, &count);

            if (event.EventType == KEY_EVENT && !event.Event.KeyEvent.bKeyDown)
            {
                switch (event.Event.KeyEvent.wVirtualKeyCode)
                {
                case VK_SPACE:
                    // call the new yell function when the spacebar is pressed
                    pGame->player->yell();
                    pGame->player->health--;
                    std::cout << "After taking 1 damage, Player's remaining health is: "
                        << pGame->player->health << std::endl;
                    break;
                case VK_ESCAPE:
                    return 0;
                }
            }
        }

        // update simulation

        // render graphics
    }
}
// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
