import { useState } from "react";
import { Button } from "../components/Button";
import { Input, Select } from "../components/Input";
import { Badge } from "../components/Badge";
import { Card, CardHeader, CardBody } from "../components/Card";
import { Table, Th, Td, Tr } from "../components/Table";
import { Tabs } from "../components/Tabs";
import { Dialog } from "../components/Dialog";
import { Toast } from "../components/Toast";
import { KpiCard } from "../components/KpiCard";

export function StyleGuidePage() {
  const [activeTab, setActiveTab] = useState("overview");
  const [dialogOpen, setDialogOpen] = useState(false);

  return (
    <div className="style-guide">
      <h2 className="section-title">Style Guide</h2>
      <p className="helper-text">
        Component primitives for the Fairline design system.
      </p>

      {/* Buttons */}
      <section className="guide-section">
        <h3 className="guide-section__title">Buttons</h3>
        <div className="guide-row">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="ghost">Ghost</Button>
          <Button variant="danger">Danger</Button>
          <Button variant="primary" disabled>
            Disabled
          </Button>
        </div>
        <div className="guide-row">
          <Button variant="primary" size="sm">
            Small
          </Button>
          <Button variant="primary" size="md">
            Medium
          </Button>
          <Button variant="primary" size="lg">
            Large
          </Button>
        </div>
      </section>

      {/* Badges */}
      <section className="guide-section">
        <h3 className="guide-section__title">Badges</h3>
        <div className="guide-row">
          <Badge variant="neutral">Neutral</Badge>
          <Badge variant="success">Value</Badge>
          <Badge variant="danger">Tax</Badge>
          <Badge variant="warning">Warning</Badge>
        </div>
      </section>

      {/* Inputs */}
      <section className="guide-section">
        <h3 className="guide-section__title">Inputs</h3>
        <div className="guide-row">
          <Input placeholder="Text input..." />
          <Select>
            <option>Select option...</option>
            <option>Option A</option>
            <option>Option B</option>
          </Select>
        </div>
      </section>

      {/* KPI Cards */}
      <section className="guide-section">
        <h3 className="guide-section__title">KPI Cards</h3>
        <div className="kpi-strip">
          <KpiCard label="Metric" value="1,247" />
          <KpiCard label="Positive" value="83" trend="up" delta="+12" />
          <KpiCard label="Negative" value="214" trend="down" delta="-8" />
          <KpiCard
            label="Neutral"
            value="2.4%"
            trend="neutral"
            delta={"\u00B10.1%"}
          />
        </div>
      </section>

      {/* Cards */}
      <section className="guide-section">
        <h3 className="guide-section__title">Cards</h3>
        <div className="guide-grid">
          <Card>
            <CardHeader>Default Card</CardHeader>
            <CardBody>Card content with neutral styling.</CardBody>
          </Card>
          <Card status="ok">
            <CardHeader>Success Card</CardHeader>
            <CardBody>Indicates a positive status.</CardBody>
          </Card>
          <Card status="error">
            <CardHeader>Error Card</CardHeader>
            <CardBody>Indicates a problem.</CardBody>
          </Card>
          <Card status="warning">
            <CardHeader>Warning Card</CardHeader>
            <CardBody>Requires attention.</CardBody>
          </Card>
        </div>
      </section>

      {/* Tabs */}
      <section className="guide-section">
        <h3 className="guide-section__title">Tabs</h3>
        <Tabs
          value={activeTab}
          onChange={setActiveTab}
          tabs={[
            { value: "overview", label: "Overview" },
            { value: "details", label: "Details" },
            { value: "history", label: "History" },
          ]}
        />
        <div className="guide-tab-content">Active tab: {activeTab}</div>
      </section>

      {/* Table */}
      <section className="guide-section">
        <h3 className="guide-section__title">Table</h3>
        <div className="card">
          <Table>
            <thead>
              <tr>
                <Th>Name</Th>
                <Th align="right">Value</Th>
                <Th>Status</Th>
              </tr>
            </thead>
            <tbody>
              <Tr variant="value">
                <Td>Sample A</Td>
                <Td align="right">+2.5%</Td>
                <Td>
                  <Badge variant="success">Value</Badge>
                </Td>
              </Tr>
              <Tr variant="tax">
                <Td>Sample B</Td>
                <Td align="right">-1.8%</Td>
                <Td>
                  <Badge variant="danger">Tax</Badge>
                </Td>
              </Tr>
              <Tr>
                <Td>Sample C</Td>
                <Td align="right">+0.1%</Td>
                <Td>
                  <Badge variant="neutral">Fair</Badge>
                </Td>
              </Tr>
            </tbody>
          </Table>
        </div>
      </section>

      {/* Dialog */}
      <section className="guide-section">
        <h3 className="guide-section__title">Dialog</h3>
        <Button variant="secondary" onClick={() => setDialogOpen(true)}>
          Open Dialog
        </Button>
        <Dialog
          open={dialogOpen}
          onClose={() => setDialogOpen(false)}
          title="Sample Dialog"
        >
          <p>This is a basic dialog component.</p>
          <div className="dialog__footer">
            <Button variant="ghost" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button variant="primary" onClick={() => setDialogOpen(false)}>
              Confirm
            </Button>
          </div>
        </Dialog>
      </section>

      {/* Toasts */}
      <section className="guide-section">
        <h3 className="guide-section__title">Toasts</h3>
        <div className="guide-stack">
          <Toast message="Operation completed successfully." variant="success" />
          <Toast message="Something went wrong." variant="danger" />
          <Toast message="Please review before proceeding." variant="warning" />
          <Toast message="New data available." variant="neutral" />
        </div>
      </section>
    </div>
  );
}
