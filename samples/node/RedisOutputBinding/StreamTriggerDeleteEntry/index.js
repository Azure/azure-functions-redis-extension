module.exports = async function (context, entry) {
    context.log("Stream entry from key 'streamTest2' with Id '" + entry.Id + "' and values '" + toString(entry.Values) + "'");
    return "streamTest2 " + entry.Id
}