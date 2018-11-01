import numpy as np
import re
from collections import *
import random
import json
from scipy.spatial import distance
import copy
from pythonosc import osc_message_builder
from pythonosc import udp_client
from pythonosc import dispatcher
from pythonosc import osc_server
from pythonosc import osc_bundle
from pythonosc import osc_bundle_builder
from pythonosc import osc_packet
from pythonosc import osc_message
import threading

# Just sound names
pathToSounds = "audiokeys_sounds.txt"

# Sounds clubbed by scripts
pathToUserSounds = "user_sounds.txt"

# AudioNotebook file names
pathToFileNames = "filenames.txt"

# AudioNotebook file names
pathToAudioKeyFileNames = "audiokeys_filenames.txt"

# a_samples = np.load('samples.npy')
a_fingerprints = np.load('fingerprints.npy')

# IP address to send data to.
remoteIPAddress = "127.0.0.1"

# port to send data to.
remotePort = 5005

# IP address of server.
localIPAddress = "127.0.0.1"

# Port of server.
localPort = 5006

scripts = []
dictList = {}
fingerprints = {}
akfdict = {}


def file_naming(n):
    new_word = ''
    for character in n:
        if character.islower():
            character = character.upper()
        if character == '-' or character == ' ':
            character = '_'
        new_word += character
    return new_word


def sort_dict(d):
    d = OrderedDict(sorted(d.items(), key=lambda t: t[1]))
    d = OrderedDict(reversed(list(d.items())))
    return dict(d)


# all fingerprints
with open(pathToFileNames, 'r') as f:
	with open(pathToAudioKeyFileNames, 'r') as akf:
	    for akfline in akf:
	    	audiokeyentry = akfline.split(",")
	    	akfdict.update({str(audiokeyentry[1]).replace('\n', ''): str(audiokeyentry[0])})
	    	
	    	# print('File name: ', audiokeyentry[1])
	    	# print('Audiokey: ', akfdict[audiokeyentry[1].replace('\n', '')])

	    place = 0
	    for line in f:
	        line = line.replace('\n', '')
	        # print('File name: ', line)
	        if(line in akfdict):
	        # print('1:', name)
	        # name = file_naming(name)
	        # print('2:', name)
	        # name = name.split("/")
	        # name = name[len(name) - 1]
	        # name = name[0:-4]
	        # print('3:', name)
		        name = akfdict[line]
		        # print('Audiokey: ', name)
	        	fingerprints.update({name: list(a_fingerprints[place])})
	        # else:
	        	# print('Filename', line, 'not found in akfdict')
	        place += 1

# list of all samples found in fingerprints
with open(pathToSounds, 'r') as f:
    for line in f:
        name = line.replace('\n', '')
        if name in fingerprints.keys():
            dictList.update({name: 0})

# all samples used in scripts
with open(pathToUserSounds, 'r') as f:
    a_list = []
    for line in f:
        name = line.replace('\n', '')
        if name != '':
            a_list.append(name)
        else:
            scripts.append(a_list)
            a_list = []


def recommend(ans, d_length = 10, best_length=10, chosen_length=10):

    global dictList
    d_list = copy.deepcopy(dictList)

    for i in scripts:
        found = 0
        for j in i:
            if j in ans:
                found = 1
        if found == 1:
            for k in i:
                if k != ans and k in fingerprints.keys():
                    d_list[k] += 1

    for n in list(d_list.keys()):
        if d_list[n] == 0:
            del d_list[n]

    d_list = sort_dict(d_list)
    while len(d_list) > d_length:
        d_list.popitem()

    chosen = []
    # ten nearest neighbors for most commonly used
    #   sorted by euclidean distance
    #   random selection of best ten
    for i in range(len(d_list)):
        best_ten = {}
        sample_name = list(d_list.keys())[i]
        for key, value in fingerprints.items():
            a = np.array(value).flatten()
            b = np.array(fingerprints[sample_name]).flatten()
            dist = distance.euclidean(a, b)
            dist = abs(dist)
            best_ten.update({key: dist})
            best_ten = sort_dict(best_ten)
            best_ten = OrderedDict(reversed(list(best_ten.items())))
            while len(best_ten) > best_length:
                best_ten.popitem()
        chosen.append(random.choice(list(best_ten.keys())))

    while len(chosen) > chosen_length:
        chosen.pop(-1)

    return chosen


def send(s_n):
    choice = recommend(s_n)
    print(choice)
    ','.join(str(i) for i in choice)
    with open('recommendedSamples.json', 'w') as outfile:
        json.dump(choice, outfile)

    print('Sending: ', "/recommendations", ' : ', choice)
    client.send_message("/recommendations", choice)
    
    # faked_choice = json.loads('["cinematicscore130BPM/Cinematic-DrumPart-001.wav", "futuredubstep140BPM/FutureDub-BassWobble-140BPM-038.wav", "ukhouse120BPM/UKHouse-SoloDrumPart-019.wav", "Techno125BPM/TSFX/TechnoWhiteNoiseSFX-008.wav", "HipHop98BPM/HHTrapBeatPart/HipHopTrapBeatPart-011.wav", "edm120BPM/EDM-RaveLead-006.wav", "ukhouse120BPM/UKHouse-MainBeat-026.wav", "pop80bpm2/crash.wav", "edm120BPM/EDM-RaveLead-002.wav", "newfunk115bpm/synth-bass2.wav"]')
    # print('Sending: ', "/recommendations", ' : ', faked_choice)
    # client.send_message("/recommendations", faked_choice)

    print('Sending: ', "/selectedsamples", ' : ', sampleNames)
    client.send_message("/selectedsamples", sampleNames)

sampleNames = []
client = udp_client.SimpleUDPClient(remoteIPAddress, remotePort)
dispatcher = dispatcher.Dispatcher()
server = osc_server.ForkingOSCUDPServer((localIPAddress, localPort), dispatcher)

def add_sample(unused_addr, args):
    print('Received: ', unused_addr, ' : ', args)
    global sampleNames
    # names = str(args).split(",")
    # for i in names:
    #     sampleNames.append(i)
    sampleNames = args
    send(sampleNames)


def clear(unused_addr, args):
    print('Received: ', unused_addr, ' : ', args)
    global sampleNames
    sampleNames = []
    # send(sampleNames)


def server_shutdown(unused_addr, args):
    print('Received: ', unused_addr, ' : ', args)
    server.shutdown()

def change_mode(unused_addr, args):
    print('Received: ', unused_addr, ' : ', args)
    #TODO: ADD SOME CODE FOR HANDLING MODE CHANGE.

dispatcher.map("/samples", add_sample)
dispatcher.map("/clear", clear)
dispatcher.map("/exit", server_shutdown)
dispatcher.map("/mode", change_mode)

server_thread = threading.Thread(target=server.serve_forever)
server_thread.start()

while True:
    new_name = input('Sample Name (clear to start over,exit to quit): ')
    if new_name == "exit" or new_name == "quit":
        break
    elif new_name == "clear":
        sampleNames = []
        send(sampleNames)
    elif new_name == "print":
        print(sampleNames)
    else:
        sampleNames.append(new_name)
        send(sampleNames)

server.shutdown()
