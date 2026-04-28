import { getTaskStatusLabel } from "./taskStatus";

describe("getTaskStatusLabel", () => {
  it("maps known statuses to human readable labels", () => {
    expect(getTaskStatusLabel("New")).toBe("New");
    expect(getTaskStatusLabel("InProgress")).toBe("In progress");
    expect(getTaskStatusLabel("SubmittedToManager")).toBe("Sent to manager");
    expect(getTaskStatusLabel("ReturnedToAdmin")).toBe("Ready for review");
    expect(getTaskStatusLabel("Done")).toBe("Completed");
  });

  it("returns the original status for unknown values", () => {
    expect(getTaskStatusLabel("Blocked")).toBe("Blocked");
  });
});
