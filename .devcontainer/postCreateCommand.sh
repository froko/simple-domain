#!/bin/bash

# Make zsh the default shell
sudo chsh "$(id -un)" --shell "/usr/bin/zsh"

# Set up pnpm
pnpm setup
