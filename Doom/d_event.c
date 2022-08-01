//
// Copyright(C) 1993-1996 Id Software, Inc.
// Copyright(C) 2005-2014 Simon Howard
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
//
// DESCRIPTION: Event handling.
//
// Events are asynchronous inputs generally generated by the game user.
// Events can be discarded if no responder claims them
//

#include <stdlib.h>
#include "d_event.h"

#define MAXEVENTS 64

static event_t events[MAXEVENTS];
static int eventhead;
static int eventtail;

//
// D_PostEvent
// Called by the I/O functions when input is detected
//
void D_PostEvent (event_t* ev)
{
	events[eventhead] = *ev;
	eventhead = (eventhead + 1) % MAXEVENTS;
}

// Read an event from the queue.

event_t *D_PopEvent(void)
{
	event_t *result;

	// No more events waiting.

	if (eventtail == eventhead)
	{
		return NULL;
	}
	
	result = &events[eventtail];

	// Advance to the next event in the queue.

	eventtail = (eventtail + 1) % MAXEVENTS;

	return result;
}

