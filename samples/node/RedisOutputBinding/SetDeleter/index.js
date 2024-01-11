module.exports = async function (context, key) {
    context.log("Deleting recently SET key '" + key + "'");
    return key;
}