#!/bin/fish

set GAME_DIR "/mnt/g/Decompiled/Knightfall/Modded/Final"
set PLUGINS_DIR "$GAME_DIR/Plugins"
set PHANTOM_PLUGINS_DIR "$PLUGINS_DIR/Phantom/plugins"

clear

mkdir -p $PHANTOM_PLUGINS_DIR

dotnet build -c Release
cp Plugin/Release/net6.0/*.dll $PLUGINS_DIR
cp Cheats/Release/net6.0/*.dll $PHANTOM_PLUGINS_DIR
