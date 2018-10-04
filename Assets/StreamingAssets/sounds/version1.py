import numpy as np
import re
from collections import *
import random
import json
from scipy.spatial import distance
import copy

# Just sound names
pathToSounds = "audiokeys_sounds.txt"

# Sounds clubbed by scripts
pathToUserSounds = "user_sounds.txt"

# AudioNotebook file names
pathToFileNames = "filenames.txt"

# a_samples = np.load('samples.npy')
a_fingerprints = np.load('fingerprints.npy')

scripts = []
dictList = {}
fingerprints = {}


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
    place = 0
    for line in f:
        name = line.replace('\n', '')
        # print('1:', name)
        name = file_naming(name)
        # print('2:', name)
        name = name.split("/")
        name = name[len(name) - 1]
        name = name[0:-4]
        # print('3:', name)
        fingerprints.update({name: list(a_fingerprints[place])})
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


sampleNames = []
while True:
    new_name = input('Sample Name (clear to start over,exit to quit): ')
    if new_name == "exit" or new_name == "quit":
        break
    elif new_name == "clear":
        sampleNames = []
    elif new_name == "print":
        print(sampleNames)
    else:
        sampleNames.append(new_name)
        choice = recommend(sampleNames)
        print(choice)
        with open('recommendedSamples.json', 'w') as outfile:
            json.dump(choice, outfile)

