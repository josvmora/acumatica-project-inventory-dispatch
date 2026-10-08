<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="JE401003.aspx.cs" Inherits="Page_JE401003" Title="Dispacht" %>
<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.JE.JEDispachtEntry"
        PrimaryView="Document" HeaderDescriptionField="ProjectName">
    <CallbackCommands>
      <%-- Status and print callbacks are generated from the graph's PXAction/PXButton
           definitions. Keep their categories and toolbar placement in the graph. --%>
      <px:PXDSCallbackCommand Name="ViewIssue" Visible="False" DependOnGrid="gridIssues" />
      <px:PXDSCallbackCommand Name="AddDocument" CommitChanges="True" Visible="False" PopupPanel="panelAddDocuments" />
      <px:PXDSCallbackCommand Name="AddSelectedDocuments" CommitChanges="True" Visible="False" />
    </CallbackCommands>
  </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
  <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Document" Width="100%" Caption="Dispacht Summary" DefaultControlID="edDispachtNbr">
    <Template>
      <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="S" ></px:PXLayoutRule>
      <px:PXSelector CommitChanges="True" ID="edDispachtNbr" runat="server" DataField="DispachtNbr" NullText="&lt;NEW&gt;" AutoRefresh="True" ></px:PXSelector>
      <px:PXDateTimeEdit CommitChanges="True" ID="edDispachtDate" runat="server" DataField="DispachtDate" ></px:PXDateTimeEdit>
      <px:PXDropDown ID="edStatus" runat="server" DataField="Status" ></px:PXDropDown>
      <px:PXDateTimeEdit ID="edShipDate" runat="server" DataField="ShipDate" ></px:PXDateTimeEdit>
      <px:PXSegmentMask ID="edBranchID" runat="server" DataField="BranchID" CommitChanges="True" ></px:PXSegmentMask>
      <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" ></px:PXLayoutRule>
      <px:PXSelector CommitChanges="True" ID="edProjectID" runat="server" DataField="ProjectID" FilterByAllFields="True" Width="250px" ></px:PXSelector>
      <px:PXSelector ID="edCarrierID" runat="server" DataField="CarrierID" CommitChanges="True" Width="250px" ></px:PXSelector>
      <px:PXTextEdit ID="edDescr" runat="server" DataField="Descr" TextMode="MultiLine" Width="250px" Height="55px" ></px:PXTextEdit>
    </Template>
  </px:PXFormView>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
  <px:PXTab ID="tab" runat="server" DataSourceID="ds" DataMember="CurrentDocument" Width="100%">
    <AutoSize Enabled="True" Container="Window" ></AutoSize>
    <Items>
      <px:PXTabItem Text="Details">
        <Template>
  <px:PXGrid runat="server" ID="gridLines" SkinID="Details" Width="100%" DataSourceID="ds" AdjustPageSize="Auto">
    <AutoSize Enabled="True" Container="Parent" ></AutoSize>
    <Levels>
      <px:PXGridLevel DataMember="Lines">
        <Columns>
          <px:PXGridColumn DataField="DisplayLineNbr" Width="60px" TextAlign="Right" AllowUpdate="False" AllowSort="False" AllowFilter="False" ></px:PXGridColumn>
          <px:PXGridColumn DataField="DocType" Width="90px" ></px:PXGridColumn>
          <px:PXGridColumn DataField="OrderType" Width="90px" ></px:PXGridColumn>
          <px:PXGridColumn DataField="OrderNbr" Width="120px" ></px:PXGridColumn>
          <px:PXGridColumn DataField="Descr" Width="350px" AllowUpdate="False" ></px:PXGridColumn>
        </Columns>
      </px:PXGridLevel>
    </Levels>
    <ActionBar>
      <CustomItems>
        <px:PXToolBarButton Text="Add Documents" CommandName="AddDocument" CommandSourceID="ds" ></px:PXToolBarButton>
      </CustomItems>
      <Actions>
        <AddNew MenuVisible="False" Enabled="False" ></AddNew>
      </Actions>
    </ActionBar>
  </px:PXGrid>
        </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Materials" RepaintOnDemand="False">
        <Template>
          <px:PXGrid ID="gridMaterials" runat="server" DataSourceID="ds" SkinID="Details" Width="100%" AdjustPageSize="Auto" AllowPaging="True" AllowSearch="True">
            <AutoSize Enabled="True" Container="Parent" ></AutoSize>
            <Levels>
              <px:PXGridLevel DataMember="Materials">
                <Columns>
                  <px:PXGridColumn DataField="DispachtNbr" Width="120px" Visible="False" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="DispachtLineNbr" Width="100px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="DocType" Width="90px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="OrderType" Width="90px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="OrderNbr" Width="120px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="SourceLineNbr" Width="100px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="InventoryID" Width="160px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Descr" Width="350px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="Qty" Width="120px" TextAlign="Right" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="UOM" Width="80px" ></px:PXGridColumn>
                  <px:PXGridColumn DataField="SiteID" Width="150px" AllowUpdate="False" />
                  <px:PXGridColumn DataField="LocationID" Width="150px" AllowUpdate="False" />
                  <px:PXGridColumn DataField="DispatchQty" Width="120px" TextAlign="Right" CommitChanges="True" />
                  <px:PXGridColumn DataField="AvailableQty" Width="160px" TextAlign="Right" AllowUpdate="False" />
                </Columns>
                <Mode AllowAddNew="False" AllowUpdate="True" AllowDelete="False" ></Mode>
              </px:PXGridLevel>
            </Levels>
          </px:PXGrid>
        </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Shipping" RepaintOnDemand="False">
        <Template>
          <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="XM" ></px:PXLayoutRule>
          <px:PXLayoutRule runat="server" StartGroup="True" GroupCaption="Delivery Information" ></px:PXLayoutRule>
          <px:PXDateTimeEdit ID="edPickupDate" runat="server" DataField="PickupDate" ></px:PXDateTimeEdit>
          <px:PXDateTimeEdit ID="edDropoffDate" runat="server" DataField="DropoffDate" ></px:PXDateTimeEdit>
          <px:PXTextEdit ID="edShippingTo" runat="server" DataField="ShippingTo" Width="250px" ></px:PXTextEdit>
        </Template>
      </px:PXTabItem>
      <px:PXTabItem Text="Issues" RepaintOnDemand="False">
        <Template>
          <px:PXGrid ID="gridIssues" runat="server" DataSourceID="ds" SkinID="Inquire" Width="100%" AllowPaging="True">
            <AutoSize Enabled="True" Container="Parent" />
            <Levels>
              <px:PXGridLevel DataMember="Issues">
                <Columns>
                  <px:PXGridColumn DataField="SiteID" Width="150px" />
                  <px:PXGridColumn DataField="RefNbr" Width="150px" LinkCommand="ViewIssue" />
                  <px:PXGridColumn DataField="ReasonCode" Width="150px" />
                  <px:PXGridColumn DataField="INRegister__Status" Width="120px" />
                  <px:PXGridColumn DataField="INRegister__Hold" Type="CheckBox" Width="70px" />
                  <px:PXGridColumn DataField="INRegister__Released" Type="CheckBox" Width="80px" />
                  <px:PXGridColumn DataField="INRegister__TranDate" Width="120px" />
                  <px:PXGridColumn DataField="INRegister__TotalQty" Width="120px" />
                  <px:PXGridColumn DataField="CreatedDateTime" Width="150px" />
                </Columns>
                <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
              </px:PXGridLevel>
            </Levels>
          </px:PXGrid>
        </Template>
      </px:PXTabItem>
    </Items>
  </px:PXTab>

  <px:PXSmartPanel runat="server" ID="panelAddDocuments" Caption="Add Documents" CaptionVisible="True" Key="AddFilter" Width="820px" Height="480px" LoadOnDemand="True" AutoRepaint="True" AutoCallBack-Enabled="True" AutoCallBack-Target="gridAddRows" AutoCallBack-Command="Refresh" CallBackMode-CommitChanges="True" CallBackMode-PostData="Page">
    <px:PXFormView runat="server" ID="frmAddFilter" DataSourceID="ds" DataMember="AddFilter" SkinID="Transparent" Caption="Selection">
      <Template>
        <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="M" ></px:PXLayoutRule>
        <px:PXDropDown CommitChanges="True" ID="edAddDocType" runat="server" DataField="DocType" ></px:PXDropDown>
        <px:PXTextEdit CommitChanges="True" ID="edAddOrderNbr" runat="server" DataField="OrderNbr" ></px:PXTextEdit>
      </Template>
    </px:PXFormView>
    <px:PXGrid runat="server" ID="gridAddRows" Width="100%" Height="320px" DataSourceID="ds" SkinID="Inquire">
      <Levels>
        <px:PXGridLevel DataMember="AddRows">
          <Columns>
            <px:PXGridColumn DataField="Selected" Type="CheckBox" AllowCheckAll="True" CommitChanges="True" Width="60px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="DocType" Width="90px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="OrderType" Width="90px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="OrderNbr" Width="120px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="ProjectID" Width="130px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="ProjectID_description" Width="220px" ></px:PXGridColumn>
            <px:PXGridColumn DataField="Descr" Width="350px" ></px:PXGridColumn>
          </Columns>
          <Mode AllowAddNew="False" AllowUpdate="True" AllowDelete="False" ></Mode>
        </px:PXGridLevel>
      </Levels>
      <AutoSize Enabled="True" Container="Parent" ></AutoSize>
    </px:PXGrid>
    <px:PXPanel runat="server" ID="pnlAddButtons" SkinID="Buttons">
      <px:PXButton runat="server" ID="btnAddSelected" Text="Add" CommandName="AddSelectedDocuments" CommandSourceID="ds" ></px:PXButton>
      <px:PXButton runat="server" ID="btnAddAndClose" Text="Add and Close" DialogResult="OK" ></px:PXButton>
      <px:PXButton runat="server" ID="btnAddCancel" DialogResult="Cancel" Text="Cancel" CommandSourceID="ds" ></px:PXButton>
    </px:PXPanel>
  </px:PXSmartPanel>
</asp:Content>
