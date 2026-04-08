export function getTaskStatusLabel(status) {
  switch (status) {
    case "New":
      return "New";
    case "InProgress":
      return "In progress";
    case "SubmittedToManager":
      return "Sent to manager";
    case "ReturnedToAdmin":
      return "Ready for review";
    case "Done":
      return "Completed";
    default:
      return status;
  }
}
