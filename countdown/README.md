# Countdown
## Helper for Conferences
This program shows a countdown from n minutes ( n < 100 ) which a progress bar right of the number. Progress bar and minute display will be green when more than 5 minutes are left, red otherwise.

## Instructions
1. run `./countdowner.fs <device> <minutes>` in this directory
### Example
- `./countdowner.fs /dev/ttyACM0 35` to run a countdown from 35 minutes

## Instructions for picoterm use (old)
1. run `./terminal`
2. enter number of total minutes followed by space
3. press <c-a> <c-s> <file> <cr>, where file is the number of minutes you want to count down from.

### Examples
- 35 <c-a><c-s>35.fs<cr> will count down from 35
- 35 <c-a><c-s>25.fs<cr> will resume a 35minute countdown for its last 25 minutes


## Uploading the firmware
1. run merge in the main directory
2. run `./terminal` (also in the main directory)
3. enter `eraseflash` to remove any old firmware
4. enter <c-a><c-s>bitkanone.fs<cr> to upload the latest version of the main firmware
5. enter <c-a><c-s>countdown/countdown.fs<cr> to upload the countdown words
