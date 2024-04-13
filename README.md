WPF:

![image](https://github.com/SilentCoast/ChatTCP/assets/94042423/9d34da89-9085-46d4-b1c1-8be8bbc9b4a0)


GTK:

![AHcABaHXoxI](https://github.com/SilentCoast/ChatTCP/assets/94042423/8fad7e51-f83e-49b6-8ae3-a7b43cc80211)

# ChatTCP

ChatTCP is a simple chat application that enables communication between two users using TCP/IP. With ChatTCP, you can either start a server to host the chat or connect to an existing server by specifying the IP address. It allows real-time messaging, and in the event of a lost connection, users will be notified.

## Features

- **Real-time Communication**: Send and receive messages instantly.
- **Server and Client Mode**: Choose to either start a server or connect to an existing one.
- **Connection Status Notification**: Get notified when the connection is lost.
- **Cross-platform**: Available for both Linux and Windows platforms with versions for WPF and GTK.

## Versions

### WPF Version (Windows)

![WPF Version](https://github.com/SilentCoast/ChatTCP/assets/94042423/9d34da89-9085-46d4-b1c1-8be8bbc9b4a0)

The WPF version of ChatTCP offers a native Windows experience with a user-friendly interface.

### GTK Version (Linux)

![GTK Version](https://github.com/SilentCoast/ChatTCP/assets/94042423/8fad7e51-f83e-49b6-8ae3-a7b43cc80211)

The GTK version of ChatTCP provides compatibility with Linux systems, ensuring seamless communication on your preferred platform.

## Contributing

Contributions are welcome! If you find any bugs or have suggestions for improvements, please open an issue or submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

ChatTCP was inspired by the need for a lightweight and simple chat application for TCP/IP communication. We thank all the contributors who have helped improve this project.

## Contact

For any inquiries or support, please contact [author_name](mailto:author@example.com).

Enjoy chatting with ChatTCP! 🚀



Known problems:

-Chat is not cleaned when leaving or entering a connection

-If there are 3+ users and one of them disconnects and connects back: this user may not be up to date with other users.
