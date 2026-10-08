<%@ Page Language="C#" MasterPageFile="~/MasterPages/ListView.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="JE401004.aspx.cs" Inherits="Page_JE401004" Title="Dispacht List" %>
<%@ MasterType VirtualPath="~/MasterPages/ListView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
  <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="PX.Objects.JE.JEDispachtInq"
        PrimaryView="Results">
  </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phG" Runat="Server">
  <px:PXGrid runat="server" ID="grid" SkinID="Inquire" Width="100%" DataSourceID="ds" AdjustPageSize="Auto">
    <Levels>
      <px:PXGridLevel DataMember="Results">
        <Columns>
          <px:PXGridColumn DataField="DispachtNbr" Width="120px" AllowEdit="True" />
          <px:PXGridColumn DataField="DispachtDate" Width="120px" />
          <px:PXGridColumn DataField="Status" Width="100px" />
          <px:PXGridColumn DataField="ProjectID" Width="130px" />
          <px:PXGridColumn DataField="Descr" Width="350px" />
        </Columns>
        <Mode AllowAddNew="False" AllowUpdate="False" AllowDelete="False" />
      </px:PXGridLevel>
    </Levels>
    <AutoSize Enabled="True" Container="Window" />
  </px:PXGrid>
</asp:Content>