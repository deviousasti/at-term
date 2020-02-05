# AT Term

AT Term is a specialized terminal for experimenting with devices that support [AT commands][1] such as GSM modems, ZigBee modules or GNSS receivers. 

![at-term](https://user-images.githubusercontent.com/2375486/73879497-9a4e6a00-4882-11ea-9be8-3d5d59e9c00b.gif)

## Main Features

- Quick connect on launch
- Suggestions and tab-completion
- History
- Favorite command set
- Implied `AT` prefix
- Quick logging
- Drag and drop file upload
- Supports PMTK commands

## Usage

`at-term` is deployed as a single executable file. 

Usage from the command line:

```
 at-term <port> <baud> <settings>
```

By default it attempts to connect to `COM1` @ `115200. 8N1`, any settings not specified are taken from default settings.

## UI

As soon as you start typing, the suggestion list appears.
Use the arrow keys to select a command, or hit tab to auto-complete the first one.

![at-term1](https://user-images.githubusercontent.com/2375486/73878905-72123b80-4881-11ea-9f72-6568e600e085.gif)

Hit <kbd>return</kbd> to send the full command with `AT+` prepended. This clears the input for the next command. 

#### History

You can cycle through history with arrow keys. History persists across sessions.

[...]

## Shortcuts

#### With command bar focused

| Key | Function  |
| ------------------------------------- | --------------------- |
| <kbd>&#8593;</kbd> <kbd>&#8595;</kbd> | Cycle through history |
| <kbd>tab</kbd> | Auto-complete closest entry |
| <kbd>ctrl</kbd>+<kbd>&#8593;</kbd>         | Jump to log |
| <kbd>ctrl</kbd>+<kbd>r</kbd>          | Repeat last command |
| <kbd>ctrl</kbd>+<kbd>d</kbd>          | Add to favorites |
| <kbd>esc</kbd> | Clear command box |
| <kbd>return</kbd> | Send current |
| <kbd>ctrl</kbd> + <kbd>return</kbd> | Send raw text (without newline) |

#### With log focused

| Key | Function  |
| ------------------------------------- | --------------------- |
| <kbd>&#8593;</kbd> <kbd>&#8595;</kbd> | Move through log |
| <kbd>esc</kbd> | Returns focus to command box |
| <kbd>return</kbd> | Send selected log |
| <kbd>ctrl</kbd> + <kbd>c</kbd> | Copies selected items to clipboard |
| <kbd>ctrl</kbd> + <kbd>v</kbd> | Sends text in clipboard as one block |

#### Global shortcuts

| Key | Function  |
| ------------------------------------- | --------------------- |
| <kbd>ctrl</kbd> + <kbd>s</kbd> | Start/stop logging |
| <kbd>ctrl</kbd> + <kbd>l</kbd> | Clears log |
| <kbd>ctrl</kbd> + <kbd>+</kbd> | Increases log text size |
| <kbd>ctrl</kbd> + <kbd>-</kbd> | Decreases log text size |

[1]: https://en.wikipedia.org/wiki/Hayes_command_set