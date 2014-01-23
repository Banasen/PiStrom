PiStrøm
=======

Streaming music from the RaspberryPi to other devices in the network.


How to Install
--------------

#### Step 1 ####

Download the latest PiStrøm Executable in the [Releases](https://github.com/Banasen/PiStrom/releases) tab.

#### Step 2 ####

Unzip the file you downloaded and configure /Config/PiStrom.xml and the .xml-s in /Streams to your liking. Configuration is described below.

#### Step 3 ####

##### On Windows: #####

Simply run PiStrom.exe after you configured it to your liking.

##### On Linux: #####

To run the .exe file you need to have [Mono](http://mono-project.com/) installed. Assuming you don't have it yet, the shell commands would be:

``` Shell
$ sudo apt-get update
$ sudo apt-get install mono-runtime
$ mono PiStrom.exe
```

Elevated access for PiStrøm is only required if you're trying to play files that you normally don't have read access to.


How to Configure
----------------

#### PiStrom.xml ####

Settings for the port and the default music (which is played when none in a stream config matches the criteria) can be found inside /Config/PiStrom.xml. The format of the file looks like this:

``` XML
<PiStrom>
  <Port>1337</Port>
  <DefaultMusic>
    <File>C:\Users\Banane\Music\Benny Hill Theme.mp3</File>
    <Folder>C:\Users\Banane\Music\Danjyon Kimura</Folder>
  </DefaultMusic>
</PiStrom>
```

* `<Port>` is the port that the Server will use.

The `<File>` and `<Folder>` tags are explained in the section about configuring the Streams, below.

**Atleast one file needs to exist in `<DefaultMusic>` for PiStrøm to start.**

#### Content of /Streams ####

This folder contains the informations for the individual streams that will be available. Each one got it's own file and will be accessed by just the filename (without .xml  at the end) in the URL. The format of the file looks like this:

``` XML
<StreamInfo>
  <Name>Electro Stream</Name>
  <Genre>Electro</Genre>
  <MetaInt>65536</MetaInt>
  <TargetByteRate>25000</TargetByteRate>
  <Music FileType="mp3">
    <TimeSpan From="02:00" Till="21:40">
      <Folder>C:\Users\Banane\Music\Binärpilot</Folder>
      <Folder>C:\Users\Banane\Music\Disasterpeace</Folder>
	  <File>C:\Users\Banane\Music\Popcorn\Hot Butter - Popcorn.mp3</File>
    </TimeSpan>
    <TimeSpan From="21:41" Till="23:59">
      <Folder>C:\Users\Banane\Music\Chipzel</Folder>
    </TimeSpan>
  </Music>
</StreamInfo>
```

While it is a bit more complex than `PiStrom.xml`, it's easy to understand as well.

* `<Name>` is the Name of the Stream.
* `<Genre>` is the Genre of the music in the Stream.

* `<MetaInt>` is the interval (in bytes) at which MetaInfo will be embedded into the Stream. Also defines the size of the buffer into which is read from the file and from which is written into the Stream.

* `<TargetByteRate>` is the maximum rate at which data is sent. In bytes per second. To get the value from the kbit/s of the music, divide by eight and then multiply by a thousand. For example 192kbit/s would require 24000 byte/s.

**Note that, if the connection isn't at top speed and the file requires the limit it might get a bit laggy and the player stops, to buffer some data. To prevent this, simply set the value higher than the maximum required value.**

Now, the values for Name and Genre only matter if the Client requests `Ice-MetaInt: 1` as a HTTP header in the request. The MetaInfo will also only be sent if that was requested, but it still defines the buffer size.

On, to the `<Music>` tag:

* `FileType` is the extension of the music files it should look for in the `<Folder>`s (it will also check the `<File>`s). This is there, because clients usually ignore content that isn't in the format of the first file. So if you were to send a .ogg after a .mp3 it would ignore it until it gets .mp3 again or stop completely.

Then comes an **unbounded** list of `<TimeSpan>` elements with this two attributes.

* `From` the time at which this `<TimeSpan>` starts.
* `Till` the time after which this `<TimeSpan>` is over.

**The values for both need to be in hh:mm 24-hour format. They have to be in the range from 00:00 to 23:59. Both values are including the minute they mark into the `<TimeSpan>`.**

This means that one going from 00:00 till 01:00 and another one starting at 01:00 and going till 02:00 would overlap at the minute 01:00.

Each `<TimeSpan>` can contain an **unbounded** list of `<File>` and/or `<Folder>` Elements.

* `<File>` is a Path to a music file.
* `<Folder>` is a Path to a folder of which **all** subfolders will be searched through for music files matching the `FileType`.

**The paths have to be either relative to the execution folder or absolute. You have to use the correct Directory Separator for your OS.**

For Windows that's the Backslash `\` and for Linux it's the (forward) Slash `/`.


Try It
------

You can find a sample stream at: http://sp.svennp.com:1337/Binaerpilot


Feedback
--------

It would be great if we could get some Feedback on this regarding features and bugs (in case there are any).

Just file a feature request or issue in the Issues tab and we'll fix the bug and take a look at adding the feature.